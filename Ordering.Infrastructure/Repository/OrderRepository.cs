using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Interfaces;  // Now from Domain
using Ordering.Domain;
using Ordering.Infrastructure.Persistence;

namespace Ordering.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderingDbContext _db;

        public OrderRepository(OrderingDbContext db)
        {
            _db = db;
        }

        public async Task CreateAsync(Order order, CancellationToken ct)
        {
            await _db.Orders.AddAsync(order, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<Order?> GetAsync(Guid id, CancellationToken ct)
        {
            return await _db.Orders
                .Include(o => o.Lines)
                .FirstOrDefaultAsync(o => o.Id == id, ct);
        }

        public async Task UpdateAsync(Order order, CancellationToken ct)
        {
            _db.Orders.Update(order);
            await _db.SaveChangesAsync(ct);
        }
    }
}