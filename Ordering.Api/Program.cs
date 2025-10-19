using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Serilog;
using Ordering.Api;
using Ordering.Infrastructure;
using Ordering.Application;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ordering API", Version = "v1" });
});

// Health checks
builder.Services.AddHealthChecks();

// Application & Infrastructure DI
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// OpenTelemetry + Prometheus (basic)
builder.Services.AddOpenTelemetry();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready", new HealthCheckOptions());

app.MapControllers();

app.Run();

namespace Ordering.Api
{
    public static class Observability
    {
        public static IServiceCollection AddOpenTelemetry(this IServiceCollection services)
        {
            // Intentionally minimal; add tracing/exporters as needed
            return services;
        }
    }
}
