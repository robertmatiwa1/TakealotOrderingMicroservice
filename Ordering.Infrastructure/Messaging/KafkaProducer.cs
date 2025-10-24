using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ordering.Infrastructure.Messaging;

public interface IKafkaProducer
{
    Task PublishAsync(string topic, string payload, CancellationToken ct);
}

public class KafkaOptions
{
    public string BootstrapServers { get; set; } = "localhost:19092";
    public string DefaultTopic { get; set; } = "ordering-events";
}

public class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;
    private readonly KafkaOptions _options;

    public KafkaProducer(ILogger<KafkaProducer> logger, IOptions<KafkaOptions> options)
    {
        _logger = logger;
        _options = options.Value;
        var cfg = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true
        };
        _producer = new ProducerBuilder<Null, string>(cfg).Build();
    }

    public async Task PublishAsync(string topic, string payload, CancellationToken ct)
    {
        topic = string.IsNullOrWhiteSpace(topic) ? _options.DefaultTopic : topic;
        var result = await _producer.ProduceAsync(topic, new Message<Null, string> { Value = payload }, ct);
        _logger.LogInformation("Published to {Topic} @ {Offset}", result.Topic, result.Offset);
    }

    public void Dispose() => _producer?.Dispose();
}
