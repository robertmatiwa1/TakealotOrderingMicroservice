using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using HealthChecks.UI.Client;
using Microsoft.EntityFrameworkCore;
using MediatR;
using System.Reflection;
using Ordering.Api.HealthChecks;
using Ordering.Application;
using Ordering.Infrastructure;
using Ordering.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ---------- Core Services ----------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ---------- Swagger ----------
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Takealot Ordering API",
        Version = "v1.0.0",
        Description = "Production-ready ordering microservice with health monitoring",
        Contact = new OpenApiContact
        {
            Name = "Takealot Engineering",
            Email = "engineering@takealot.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });
    c.EnableAnnotations();
});

// ---------- MediatR ----------
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.RegisterServicesFromAssembly(typeof(Ordering.Application.DependencyInjection).Assembly);
});

// ---------- Configuration ----------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? throw new InvalidOperationException("Database connection string not configured.");

var kafkaBootstrap = builder.Configuration["Kafka:BootstrapServers"]
    ?? Environment.GetEnvironmentVariable("Kafka__BootstrapServers")
    ?? throw new InvalidOperationException("Kafka bootstrap not configured.");

var port = Environment.GetEnvironmentVariable("PORT") ?? "5055";

// ---------- Infrastructure ----------
builder.Services
    .AddApplication()
    .AddInfrastructure(connectionString, kafkaBootstrap, "ordering-outbox");

// ---------- Health Checks ----------
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql", tags: new[] { "database", "ready" })
    .AddCheck<OrderingServiceHealthCheck>("ordering-service", tags: new[] { "service", "ready" });

builder.Services.AddHealthChecksUI(setup =>
{
    setup.AddHealthCheckEndpoint("Ordering API", $"/health");
    setup.SetEvaluationTimeInSeconds(60);
    setup.MaximumHistoryEntriesPerEndpoint(50);
})
.AddInMemoryStorage();

// ---------- CORS ----------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin());
});

var app = builder.Build();

// ---------- Middleware ----------
app.UseRouting();
app.UseCors("AllowAll");

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Takealot Ordering API v1");
        c.RoutePrefix = "api-docs";
        c.DisplayRequestDuration();
    });
}

app.UseStaticFiles();
app.UseAuthorization();

// ---------- Health Endpoints ----------
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecksUI(options => options.UIPath = "/health-ui");

// ---------- Controllers ----------
app.MapControllers();

// ---------- Root Diagnostic Endpoint ----------
app.MapGet("/", () => new
{
    service = "Takealot Ordering API",
    version = "1.0.0",
    status = "operational",
    environment = app.Environment.EnvironmentName,
    timestamp = DateTime.UtcNow,
    links = new
    {
        health = "/health",
        ready = "/health/ready",
        live = "/health/live",
        docs = "/api-docs",
        health_ui = "/health-ui"
    }
});

// ---------- Database Migration ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
    try
    {
        if (app.Environment.IsDevelopment())
        {
            await db.Database.MigrateAsync();
            Console.WriteLine("Database migration completed successfully.");
        }
        else if (await db.Database.CanConnectAsync())
        {
            Console.WriteLine("Database connection successful.");
        }
        else
        {
            Console.WriteLine("Unable to connect to database.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization failed: {ex.Message}");
        if (app.Environment.IsDevelopment()) throw;
    }
}

// ---------- Startup Info ----------
Console.WriteLine("\n==================================================");
Console.WriteLine("Takealot Ordering API Started");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"Port: {port}");
Console.WriteLine($"Database: {(string.IsNullOrEmpty(connectionString) ? "Missing" : "Configured")}");
Console.WriteLine($"Kafka: {(string.IsNullOrEmpty(kafkaBootstrap) ? "Missing" : "Configured")}");
Console.WriteLine("Endpoints:");
Console.WriteLine("  /               - API Root");
Console.WriteLine("  /api-docs       - Swagger UI");
Console.WriteLine("  /health*        - Health Checks");
Console.WriteLine("  /health-ui      - Health Dashboard");
Console.WriteLine("==================================================\n");

app.Run($"http://0.0.0.0:{port}");
