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

// MVC + Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger (registered once, enabled conditionally later)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Takealot Ordering API",
        Version = "v1.0.0",
        Description = "Production-ready ordering microservice",
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

    // JWT Bearer Security
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// MediatR (optional but kept for CQRS or domain event patterns)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.RegisterServicesFromAssembly(typeof(Ordering.Application.DependencyInjection).Assembly);
});

// Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=postgres;Port=5432;Database=ordering;Username=postgres;Password=postgres123;";
var kafkaBootstrap = builder.Configuration["Kafka:BootstrapServers"] ?? "redpanda:9092";

// Application + Infrastructure layers
builder.Services
    .AddApplication()
    .AddInfrastructure(connectionString, kafkaBootstrap, "ordering-outbox");

// ---------- Health Checks ----------
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql", tags: new[] { "database", "ready" })
    // Uncomment if Redis is used in your infra
    //.AddRedis(builder.Configuration["Redis:ConnectionString"], name: "redis-cache", tags: new[] { "cache", "ready" })
    .AddCheck<OrderingServiceHealthCheck>("ordering-service", tags: new[] { "service", "ready" });

// HealthChecks UI - Relative path ensures it works across environments
builder.Services.AddHealthChecksUI(setup =>
{
    setup.DisableDatabaseMigrations();
    setup.MaximumHistoryEntriesPerEndpoint(50);
})
.AddInMemoryStorage();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("HealthChecks", policy =>
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin());
});

var app = builder.Build();

// ---------- Middleware Configuration ----------

// HTTPS, static files for HealthChecks UI
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("HealthChecks");
app.UseAuthorization();

// ---------- Swagger UI (conditional) ----------
if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Takealot Ordering API v1");
        c.RoutePrefix = "api-docs";
        c.DocumentTitle = "Takealot Ordering API Portal";
        c.DisplayRequestDuration();
        c.DisplayOperationId();
    });
}

// ---------- Health Endpoints ----------
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    AllowCachingResponses = false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    AllowCachingResponses = false
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    AllowCachingResponses = false
});

// HealthChecks UI (Dashboard)
app.MapHealthChecksUI(setup => { setup.UIPath = "/health-ui"; });

// ---------- API Controllers ----------
app.MapControllers();

// ---------- Minimal Diagnostic Endpoints ----------
app.MapGet("/", () => new
{
    service = "Takealot Ordering API",
    version = "1.0.0",
    status = "operational",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
});

app.MapGet("/test", () => new
{
    message = "Service is responsive",
    timestamp = DateTime.UtcNow
});

// ---------- Database Migration on Startup ----------
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
Console.WriteLine($"Swagger UI: http://localhost:5055/api-docs");
Console.WriteLine($"Health UI: http://localhost:5055/health-ui");

app.Run("http://0.0.0.0:5055");
