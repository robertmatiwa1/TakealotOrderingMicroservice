using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ordering.Infrastructure.Persistence;
using Ordering.Infrastructure.Messaging;

namespace Ordering.Infrastructure.Outbox;

public class OutboxDispatcher : BackgroundService
{
    private readonly ILogger<OutboxDispatcher> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IKafkaProducer _producer;

    public OutboxDispatcher(
        ILogger<OutboxDispatcher> logger, 
        IServiceScopeFactory scopeFactory, 
        IKafkaProducer producer)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _producer = producer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxDispatcher started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Create a new scope for each iteration
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
                
                var messages = await db.OutboxMessages
                    .Where(m => m.DispatchedAt == null)
                    .OrderBy(m => m.OccurredAt)
                    .Take(50)
                    .ToListAsync(stoppingToken);

                foreach (var message in messages)
                {
                    try
                    {
                        await _producer.PublishAsync(message.Topic, message.Payload, stoppingToken);
                        message.DispatchedAt = DateTimeOffset.UtcNow;
                        _logger.LogDebug("Successfully published outbox message {MessageId}", message.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to publish outbox message {MessageId}", message.Id);
                        // Consider adding retry logic or dead letter queue here
                    }
                }

                if (messages.Count > 0)
                {
                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Processed {MessageCount} outbox messages", messages.Count);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error during outbox processing cycle");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
        
        _logger.LogInformation("OutboxDispatcher stopped");
    }
}