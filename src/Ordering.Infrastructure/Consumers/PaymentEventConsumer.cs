using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Ordering.Domain;
using Ordering.Domain.Events;
using Ordering.Infrastructure.Persistence;
using System.Text.Json;

namespace Ordering.Infrastructure.Consumers
{
    public class PaymentEventConsumer : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<PaymentEventConsumer> _logger;
        private readonly string _bootstrap;
        private readonly string _topic;

        public PaymentEventConsumer(IServiceProvider sp, ILogger<PaymentEventConsumer> logger, IConfiguration cfg)
        {
            _sp = sp; _logger = logger;
            _bootstrap = cfg.GetSection("Kafka")["BootstrapServers"] ?? "localhost:9092";
            _topic = cfg.GetSection("Kafka")["PaymentTopic"] ?? "payment-events";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var conf = new ConsumerConfig {
                BootstrapServers = _bootstrap,
                GroupId = "ordering-payment-consumer",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };

            using var consumer = new ConsumerBuilder<string,string>(conf).Build();
            consumer.Subscribe(_topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cr = consumer.Consume(stoppingToken);
                    if (cr is null || string.IsNullOrWhiteSpace(cr.Message.Value)) continue;

                    var envelope = JsonSerializer.Deserialize<JsonElement>(cr.Message.Value);
                    var eventType = envelope.GetProperty("eventType").GetString();

                    using var scope = _sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    if (eventType == nameof(PaymentSucceeded))
                    {
                        var data = envelope.GetProperty("data");
                        var orderId = data.GetProperty("orderId").GetGuid();
                        var order = await db.Orders.Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == orderId, stoppingToken);
                        if (order != null)
                        {
                            order.Accept();
                            await db.SaveChangesAsync(stoppingToken);
                        }
                    }
                    else if (eventType == nameof(PaymentFailed))
                    {
                        var data = envelope.GetProperty("data");
                        var orderId = data.GetProperty("orderId").GetGuid();
                        var reason = data.GetProperty("reason").GetString() ?? "PaymentFailed";
                        var order = await db.Orders.Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == orderId, stoppingToken);
                        if (order != null)
                        {
                            order.Cancel(reason);
                            await db.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error consuming payment event");
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
    }
}
