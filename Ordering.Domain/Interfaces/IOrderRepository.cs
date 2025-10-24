using Ordering.Domain;

namespace Ordering.Domain.Interfaces
{
    public interface IOrderRepository
    {
        Task CreateAsync(Order order, CancellationToken ct);
        Task<Order?> GetAsync(Guid id, CancellationToken ct);
        Task UpdateAsync(Order order, CancellationToken ct);
    }
}