using System.Text.Json;

namespace Ordering.Infrastructure.Persistence
{
    public class OutboxMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Type { get; set; } = default!;
        public string Payload { get; set; } = default!;
        public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? PublishedUtc { get; set; }
    }
}
