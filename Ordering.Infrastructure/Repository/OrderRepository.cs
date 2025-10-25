using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ordering.Domain;
using Ordering.Domain.Interfaces;
using Ordering.Infrastructure.Persistence;

namespace Ordering.Infrastructure.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderingDbContext _dbContext;

        public OrderRepository(OrderingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Order?> GetAsync(Guid id, CancellationToken ct)
        {
            return await _dbContext.Orders
                .Include(o => o.Lines)
                .FirstOrDefaultAsync(o => o.Id == id, ct);
        }

        public async Task CreateAsync(Order order, CancellationToken ct)
        {
            await _dbContext.Orders.AddAsync(order, ct);
            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Order order, CancellationToken ct)
        {
            _dbContext.Orders.Update(order);
            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            var order = await _dbContext.Orders.FindAsync([id], ct);
            if (order is null) return;

            _dbContext.Orders.Remove(order);
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
