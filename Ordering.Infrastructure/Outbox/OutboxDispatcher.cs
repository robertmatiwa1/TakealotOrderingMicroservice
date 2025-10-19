using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Ordering.Infrastructure.Messaging;
using Ordering.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Ordering.Infrastructure.Outbox
{
    public class OutboxDispatcher : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxDispatcher> _logger;
        private readonly string _topic;

        public OutboxDispatcher(IServiceScopeFactory scopeFactory, ILogger<OutboxDispatcher> logger, IConfiguration config)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _topic = config.GetSection("Kafka")["Topic"] ?? "ordering-events";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var bus = scope.ServiceProvider.GetRequiredService<IEventBus>();

                    var batch = await db.OutboxMessages
                        .Where(x => x.PublishedUtc == null)
                        .OrderBy(x => x.OccurredAtUtc)
                        .Take(50)
                        .ToListAsync(stoppingToken);

                    foreach (var msg in batch)
                    {
                        await bus.PublishAsync(_topic, msg.Id.ToString(), msg.Payload, stoppingToken);
                        msg.PublishedUtc = DateTime.UtcNow;
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Outbox dispatch error");
                }

                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
        }
    }
}
