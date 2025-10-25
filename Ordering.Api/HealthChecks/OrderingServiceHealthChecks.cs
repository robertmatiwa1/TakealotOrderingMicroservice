using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Ordering.Infrastructure.Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ordering.Api.HealthChecks
{
    public class OrderingServiceHealthCheck : IHealthCheck
    {
        private readonly OrderingDbContext _dbContext;
        private readonly ILogger<OrderingServiceHealthCheck> _logger;

        public OrderingServiceHealthCheck(OrderingDbContext dbContext, ILogger<OrderingServiceHealthCheck> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
                if (canConnect)
                    return HealthCheckResult.Healthy("Database connection successful");

                return HealthCheckResult.Unhealthy("Database connection failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ordering service health check failed");
                return HealthCheckResult.Unhealthy("Ordering service health check failed", ex);
            }
        }
    }
}
