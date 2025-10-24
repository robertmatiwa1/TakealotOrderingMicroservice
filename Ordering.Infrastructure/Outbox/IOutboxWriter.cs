namespace Ordering.Infrastructure.Outbox
{
    public interface IOutboxWriter
    {
        Task WriteAsync(object domainEvent, CancellationToken ct);
    }
}
