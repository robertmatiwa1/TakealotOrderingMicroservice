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

// ---------- Application + Infrastructure ----------
builder.Services
    .AddApplication()
    .AddInfrastructure(
    connectionString ?? throw new InvalidOperationException("Connection string not configured."),
    kafkaBootstrap ?? throw new InvalidOperationException("Kafka bootstrap not configured."),
    "ordering-outbox"
);

// ---------- Health Checks ----------
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql", tags: new[] { "database", "ready" })
    .AddCheck<OrderingServiceHealthCheck>("ordering-service", tags: new[] { "service", "ready" });

// Health Checks UI - Configured for Docker environment
builder.Services.AddHealthChecksUI(setup =>
{
    // In Docker, health checks are typically managed by an external aggregator
    // Remove self-referential checks to prevent circular dependencies
    if (builder.Environment.IsDevelopment())
    {
        // Only for local development - monitor self
        setup.AddHealthCheckEndpoint("Ordering API", $"http://host.docker.internal:{port}/health");
    }
    // Production: Health checks should be configured externally via environment variables
    setup.SetEvaluationTimeInSeconds(60);
    setup.MinimumSecondsBetweenFailureNotifications(30);
    setup.MaximumHistoryEntriesPerEndpoint(50);
    
    // Configure webhooks for notifications (optional)
    setup.AddWebhookNotification("Slack", 
        webhookUri: "https://hooks.slack.com/services/your-webhook",
        payload: "{\"text\": \"[[LIVENESS]] Health Check for [[DESCRIPTIONS]] has failed!\"}",
        restorePayload: "{\"text\": \"[[LIVENESS]] Health Check for [[DESCRIPTIONS]] is recovered!\"}");
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

// ---------- Middleware Pipeline ----------
app.UseRouting();

// Global CORS - Apply before other middleware
app.UseCors("AllowAll");

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
    
    // Swagger only in non-production
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

// ---------- Health Check Endpoints ----------
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

// Health Checks UI Dashboard
app.MapHealthChecksUI(setup => 
{ 
    setup.UIPath = "/health-ui";
    setup.AddCustomStylesheet("health-checks.css");
});

// ---------- API Controllers ----------
app.MapControllers();

// ---------- Root Endpoint ----------
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
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        
        // Only auto-migrate in development
        if (app.Environment.IsDevelopment())
        {
            await db.Database.MigrateAsync();
            Console.WriteLine("Database migration completed successfully.");
        }
        else
        {
            // In production, just test connection
            var canConnect = await db.Database.CanConnectAsync();
            Console.WriteLine($"Database connection: {(canConnect ? "OK" : "FAILED")}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization failed: {ex.Message}");
        // Don't crash the app in production
        if (app.Environment.IsDevelopment())
            throw;
    }
}

// ---------- Startup Logging ----------
Console.WriteLine("==================================================");
Console.WriteLine("Takealot Ordering API - Docker Optimized");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"Port: {port}");
Console.WriteLine($"Database: {(!string.IsNullOrEmpty(connectionString) ? "Configured" : "MISSING")}");
Console.WriteLine($"Kafka: {(!string.IsNullOrEmpty(kafkaBootstrap) ? "Configured" : "MISSING")}");
Console.WriteLine("Endpoints:");
Console.WriteLine("  - HTTP: /");
Console.WriteLine("  - Health: /health, /health/ready, /health/live");
Console.WriteLine("  - Monitoring: /health-ui");
Console.WriteLine("  - Documentation: /api-docs");
Console.WriteLine("==================================================");

// Start the application - crucial for Docker
app.Run($"http://0.0.0.0:{port}");