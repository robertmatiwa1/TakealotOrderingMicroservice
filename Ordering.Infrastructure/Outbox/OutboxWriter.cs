using Ordering.Infrastructure.Persistence;

namespace Ordering.Infrastructure.Outbox
{
    public class OutboxWriter : IOutboxWriter
    {
        private readonly OrderingContext _context;

        public OutboxWriter(OrderingContext context)
        {
            _context = context;
        }

        public Task WriteAsync(object domainEvent, CancellationToken ct)
        {
            // Implementation for writing to outbox
            // For now, just return completed task
            return Task.CompletedTask;
        }
    }
}
