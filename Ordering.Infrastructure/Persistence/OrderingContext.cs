using Microsoft.EntityFrameworkCore;
using Ordering.Domain;
using Ordering.Domain.Interfaces;

namespace Ordering.Infrastructure.Persistence
{
    public class OrderingContext : DbContext, IOrderRepository
    {
        public OrderingContext(DbContextOptions<OrderingContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders => Set<Order>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderingContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            return await Orders
                .Include(o => o.Lines)
                .FirstOrDefaultAsync(o => o.Id == id, ct);
        }

        public async Task<Order?> GetAsync(Guid id, CancellationToken ct)
        {
            return await GetByIdAsync(id, ct);
        }

        public async Task CreateAsync(Order order, CancellationToken ct)
        {
            await Orders.AddAsync(order, ct);
            await SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Order order, CancellationToken ct)
        {
            Orders.Update(order);
            await SaveChangesAsync(ct);
        }

        public async Task AddAsync(Order order, CancellationToken ct)
        {
            await Orders.AddAsync(order, ct);
            await SaveChangesAsync(ct);
        }
    }
}
