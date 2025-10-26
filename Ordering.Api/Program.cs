using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Ordering.Api.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.EntityFrameworkCore;
using Ordering.Application;
using Ordering.Infrastructure;
using Ordering.Infrastructure.Persistence;
using MediatR;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// ---------- Service Registration ----------
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
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection") ??
    Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

var kafkaBootstrap =
    builder.Configuration["Kafka:BootstrapServers"] ??
    Environment.GetEnvironmentVariable("Kafka__BootstrapServers");

var port = Environment.GetEnvironmentVariable("PORT") ?? "5055";
var healthEndpoint = $"http://127.0.0.1:{port}/health"; // works inside container

// ---------- Application + Infrastructure ----------
builder.Services
    .AddApplication()
    .AddInfrastructure(connectionString, kafkaBootstrap, "ordering-outbox");

// ---------- Health Checks ----------
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql", tags: new[] { "database", "ready" })
    .AddCheck<OrderingServiceHealthCheck>("ordering-service", tags: new[] { "service", "ready" });

builder.Services.AddHealthChecksUI(setup =>
{
    setup.AddHealthCheckEndpoint("Ordering API", healthEndpoint);
    setup.SetEvaluationTimeInSeconds(60);
    setup.MaximumHistoryEntriesPerEndpoint(50);
})
.AddInMemoryStorage();

// ---------- CORS ----------
builder.Services.AddCors(options =>
{
    options.AddPolicy("HealthChecks", policy =>
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin());
});

var app = builder.Build();

// ---------- Middleware ----------
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseCors("HealthChecks");
app.UseAuthorization();

// ---------- Swagger ----------
if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Takealot Ordering API v1");
        c.RoutePrefix = "api-docs";
    });
}

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

app.MapHealthChecksUI(setup => { setup.UIPath = "/health-ui"; });

// ---------- Controllers ----------
app.MapControllers();

// ---------- Diagnostic ----------
app.MapGet("/", () => new
{
    service = "Takealot Ordering API",
    version = "1.0.0",
    status = "operational",
    environment = app.Environment.EnvironmentName,
    timestamp = DateTime.UtcNow
});

// ---------- Database Migration ----------
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        db.Database.Migrate();
        Console.WriteLine("Database migration completed successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration failed: {ex.Message}");
    }
}

Console.WriteLine("Takealot Ordering API started successfully!");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"Health Endpoint: {healthEndpoint}");
Console.WriteLine("--------------------------------------------------");

app.Run($"http://0.0.0.0:{port}");
