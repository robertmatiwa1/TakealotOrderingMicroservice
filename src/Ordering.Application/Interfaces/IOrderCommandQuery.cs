using Ordering.Domain;

namespace Ordering.Application.Interfaces
{
    public interface IOrderCommandService
    {
        Task<Order> CreateAsync(Guid customerId, List<(string sku, int quantity, decimal unitPrice)> lines, CancellationToken ct);
        Task CancelAsync(Guid id, string reason, CancellationToken ct);
        Task AcceptAsync(Guid id, CancellationToken ct);
        Task CompleteAsync(Guid id, CancellationToken ct);
    }

    public interface IOrderQueryService
    {
        Task<Order?> GetAsync(Guid id, CancellationToken ct);
        Task<IReadOnlyList<Order>> ListByCustomerAsync(Guid customerId, CancellationToken ct);
    }
}
