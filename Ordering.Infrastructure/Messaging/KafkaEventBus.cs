using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace Ordering.Infrastructure.Messaging
{
    public interface IEventBus
    {
        Task PublishAsync(string topic, string key, string payload, CancellationToken ct);
    }

    public class KafkaEventBus : IEventBus, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly string _bootstrap;
        public KafkaEventBus(IConfiguration config)
        {
            _bootstrap = config.GetSection("Kafka")["BootstrapServers"] ?? "localhost:9092";
            var conf = new ProducerConfig { BootstrapServers = _bootstrap, EnableIdempotence = true };
            _producer = new ProducerBuilder<string, string>(conf).Build();
        }

        public async Task PublishAsync(string topic, string key, string payload, CancellationToken ct)
        {
            await _producer.ProduceAsync(topic, new Message<string, string> { Key = key, Value = payload }, ct);
        }

        public void Dispose() => _producer?.Dispose();
    }
}
