using Microsoft.EntityFrameworkCore;
using Ordering.Application.Interfaces;
using Ordering.Domain;
using Ordering.Infrastructure.Outbox;
using Ordering.Infrastructure.Persistence;

namespace Ordering.Application.Services
{
    public class OrderCommandService : IOrderCommandService
    {
        private readonly AppDbContext _db;
        private readonly IOutboxWriter _outbox;

        public OrderCommandService(AppDbContext db, IOutboxWriter outbox)
        {
            _db = db; _outbox = outbox;
        }

        public async Task<Order> CreateAsync(Guid customerId, List<(string sku, int quantity, decimal unitPrice)> lines, CancellationToken ct)
        {
            var order = Order.Place(customerId, lines.Select(l => new OrderLine(l.sku, l.quantity, Money.Of(l.unitPrice, "ZAR"))).ToList());
            await _db.Orders.AddAsync(order, ct);
            await _db.SaveChangesAsync(ct);
            foreach (var evt in order.DequeueDomainEvents())
                await _outbox.WriteAsync(evt, ct);
            return order;
        }

        public async Task CancelAsync(Guid id, string reason, CancellationToken ct)
        {
            var order = await _db.Orders.Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == id, ct)
                ?? throw new InvalidOperationException("Order not found");

            order.Cancel(reason);
            await _db.SaveChangesAsync(ct);
            foreach (var evt in order.DequeueDomainEvents())
                await _outbox.WriteAsync(evt, ct);
        }

        public async Task AcceptAsync(Guid id, CancellationToken ct)
        {
            var order = await _db.Orders.Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == id, ct)
                ?? throw new InvalidOperationException("Order not found");
            order.Accept();
            await _db.SaveChangesAsync(ct);
            foreach (var evt in order.DequeueDomainEvents())
                await _outbox.WriteAsync(evt, ct);
        }

        public async Task CompleteAsync(Guid id, CancellationToken ct)
        {
            var order = await _db.Orders.Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == id, ct)
                ?? throw new InvalidOperationException("Order not found");
            order.Complete();
            await _db.SaveChangesAsync(ct);
            foreach (var evt in order.DequeueDomainEvents())
                await _outbox.WriteAsync(evt, ct);
        }
    }

    public class OrderQueryService : IOrderQueryService
    {
        private readonly AppDbContext _db;
        public OrderQueryService(AppDbContext db) => _db = db;

        public Task<Order?> GetAsync(Guid id, CancellationToken ct)
            => _db.Orders.Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == id, ct);

        public async Task<IReadOnlyList<Order>> ListByCustomerAsync(Guid customerId, CancellationToken ct)
            => await _db.Orders.AsNoTracking().Where(o => o.CustomerId == customerId).ToListAsync(ct);
    }
}
