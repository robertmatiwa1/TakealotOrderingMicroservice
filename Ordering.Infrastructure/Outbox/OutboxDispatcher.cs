using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ordering.Infrastructure.Persistence;
using Ordering.Infrastructure.Messaging;

namespace Ordering.Infrastructure.Outbox;

public class OutboxDispatcher : BackgroundService
{
    private readonly ILogger<OutboxDispatcher> _logger;
    private readonly OrderingDbContext _db;
    private readonly IKafkaProducer _producer;

    public OutboxDispatcher(ILogger<OutboxDispatcher> logger, OrderingDbContext db, IKafkaProducer producer)
    {
        _logger = logger;
        _db = db;
        _producer = producer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxDispatcher started");
        while (!stoppingToken.IsCancellationRequested)
        {
            var messages = await _db.OutboxMessages
                .Where(m => m.DispatchedAt == null)
                .OrderBy(m => m.OccurredAt)
                .Take(50)
                .ToListAsync(stoppingToken);

            foreach (var m in messages)
            {
                try
                {
                    await _producer.PublishAsync(m.Topic, m.Payload, stoppingToken);
                    m.DispatchedAt = DateTimeOffset.UtcNow;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish outbox message {Id}", m.Id);
                }
            }

            if (messages.Count > 0)
            {
                await _db.SaveChangesAsync(stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}
