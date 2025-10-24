using Ordering.Domain;

namespace Ordering.Application.Interfaces
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(Guid customerId, List<(string sku, int quantity, decimal unitPrice)> lines, CancellationToken ct);
        Task<Order?> GetAsync(Guid id, CancellationToken ct);
        Task<bool> CancelOrderAsync(Guid orderId, CancellationToken ct);
    }
}
