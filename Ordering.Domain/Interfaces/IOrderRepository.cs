using System;
using System.Threading;
using System.Threading.Tasks;
using Ordering.Domain;

namespace Ordering.Domain.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order?> GetAsync(Guid id, CancellationToken ct);     
        Task CreateAsync(Order order, CancellationToken ct);     
        Task UpdateAsync(Order order, CancellationToken ct);
        Task DeleteAsync(Guid id, CancellationToken ct);
    }
}
