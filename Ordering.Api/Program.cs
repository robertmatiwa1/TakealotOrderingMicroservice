using Microsoft.EntityFrameworkCore;
using Ordering.Application;
using Ordering.Infrastructure;
using Ordering.Infrastructure.Persistence;
using MediatR;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register MediatR from multiple assemblies
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()); // API assembly
    cfg.RegisterServicesFromAssembly(typeof(Ordering.Application.DependencyInjection).Assembly); // Application assembly
});

// Get configuration values - UPDATED connection string with correct password
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=postgres;Port=5432;Database=ordering;Username=postgres;Password=postgres123;";

var kafkaBootstrap = builder.Configuration["Kafka:BootstrapServers"] 
    ?? "redpanda:9092";

// Add application and infrastructure services with required parameters
builder.Services
    .AddApplication()
    .AddInfrastructure(connectionString, kafkaBootstrap, "ordering-outbox");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// Add a simple minimal API endpoint for testing
app.MapGet("/", () => "Ordering API is running!");
app.MapGet("/test", () => new { message = "Test endpoint works!", timestamp = DateTime.UtcNow });

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<OrderingContext>();
        db.Database.Migrate();
        Console.WriteLine(" Database migration completed successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($" Database operation failed: {ex.Message}");
    }
}

Console.WriteLine(" Ordering API started successfully!");

// Use the URL from environment variable or default to 5055
var url = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://0.0.0.0:5055";
Console.WriteLine($" Starting API on: {url}");
app.Run(url);
