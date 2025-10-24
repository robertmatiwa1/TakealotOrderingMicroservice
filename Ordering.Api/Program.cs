using System.Reflection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Ordering.Infrastructure;
using Ordering.Infrastructure.Persistence;
using Ordering.Infrastructure.Outbox;
using Ordering.Infrastructure.Messaging;
using Ordering.Application;
using Ordering.Application.Commands;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// Config
var connectionString = builder.Configuration.GetConnectionString("OrderingDb")
    ?? builder.Configuration["ConnectionStrings:OrderingDb"]
    ?? "Host=localhost;Database=ordering;Username=postgres;Password=password";

var kafkaBootstrap = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:19092";
var kafkaTopic = builder.Configuration["Kafka:Topic"] ?? "ordering-events";

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ordering API", Version = "v1" });
});

builder.Services.AddHealthChecks();

builder.Services.AddDbContext<OrderingDbContext>(opts =>
    opts.UseNpgsql(connectionString));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddApplication(); // extension method in Application to register validators/behaviors
builder.Services.AddInfrastructure(connectionString, kafkaBootstrap, kafkaTopic);

// Hosted services
builder.Services.AddHostedService<OutboxDispatcher>();

var app = builder.Build();

// Auto-migrate in Development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
    db.Database.Migrate();
}

app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ordering API v1");
    c.RoutePrefix = string.Empty;
});

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready", new HealthCheckOptions());

app.MapControllers();

app.Run();
