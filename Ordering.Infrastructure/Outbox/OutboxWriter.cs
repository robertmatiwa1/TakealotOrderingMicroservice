using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ordering.Infrastructure.Persistence;

namespace Ordering.Infrastructure.Outbox
{
    public interface IOutboxWriter
    {
        Task WriteAsync(object domainEvent, CancellationToken ct);
    }

    public class OutboxWriter : IOutboxWriter
    {
        private readonly AppDbContext _db;
        public OutboxWriter(AppDbContext db) => _db = db;

        public async Task WriteAsync(object domainEvent, CancellationToken ct)
        {
            var msg = new OutboxMessage
            {
                Type = domainEvent.GetType().FullName!,
                Payload = JsonSerializer.Serialize(domainEvent),
                OccurredAtUtc = DateTime.UtcNow
            };
            await _db.OutboxMessages.AddAsync(msg, ct);
            await _db.SaveChangesAsync(ct);
        }
    }
}
