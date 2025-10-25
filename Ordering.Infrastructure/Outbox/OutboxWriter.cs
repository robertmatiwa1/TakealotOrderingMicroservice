using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ordering.Infrastructure.Persistence;

namespace Ordering.Infrastructure.Outbox
{
    public class OutboxWriter : IOutboxWriter
    {
        private readonly OrderingDbContext _context;

        public OutboxWriter(OrderingDbContext context)
        {
            _context = context;
        }

        public async Task WriteAsync(object domainEvent, CancellationToken ct)
        {
            // Serialize the event for storage
            var payload = JsonSerializer.Serialize(domainEvent);

            var message = new OutboxMessage
            {
                Topic = domainEvent.GetType().Name.ToLowerInvariant(),
                Payload = payload,
                OccurredAt = DateTimeOffset.UtcNow
            };

            await _context.OutboxMessages.AddAsync(message, ct);
            await _context.SaveChangesAsync(ct);
        }
    }
}
