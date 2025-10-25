using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ordering.Api.HealthChecks;

public class RedisHealthCheck : IHealthCheck
{
    private readonly ILogger<RedisHealthCheck> _logger;

    public RedisHealthCheck(ILogger<RedisHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            ["implementation"] = "NotConfigured",
            ["status"] = "Placeholder",
            ["timestamp"] = DateTime.UtcNow,
            ["recommendation"] = "Configure Redis for production caching and session storage"
        };

        _logger.LogInformation("Redis health check executed - service not configured");
        
        return Task.FromResult(HealthCheckResult.Degraded(
            "Redis cache not configured - using in-memory fallback",
            data: data));
    }
}