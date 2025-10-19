using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Ordering.Infrastructure.Persistence;

namespace Ordering.Infrastructure.Outbox
{
    public class OutboxRetentionJob : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<OutboxRetentionJob> _logger;

        public OutboxRetentionJob(IServiceProvider sp, ILogger<OutboxRetentionJob> logger)
        {
            _sp = sp; _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var cutoff = DateTime.UtcNow.AddDays(-7);

                    var stale = await db.OutboxMessages
                        .Where(x => x.PublishedUtc != null && x.PublishedUtc < cutoff)
                        .ExecuteDeleteAsync(stoppingToken);

                    if (stale > 0)
                        _logger.LogInformation("Outbox cleanup deleted {Count} rows", stale);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Outbox retention job error");
                }
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }
    }
}
