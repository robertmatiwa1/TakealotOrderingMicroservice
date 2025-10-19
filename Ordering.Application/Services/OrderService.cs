using Ordering.Application.Interfaces;
using Ordering.Domain;
using Ordering.Infrastructure.Outbox;
using Ordering.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ordering.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _db;
        private readonly IOutboxWriter _outbox;

        public OrderService(AppDbContext db, IOutboxWriter outbox)
        {
            _db = db;
            _outbox = outbox;
        }

        public async Task<Order> CreateOrderAsync(Guid customerId, List<(string sku, int quantity, decimal unitPrice)> lines, CancellationToken ct)
        {
            var order = Order.Place(customerId, lines.Select(l => new OrderLine(l.sku, l.quantity, Money.Of(l.unitPrice, "ZAR"))).ToList());
            await _db.Orders.AddAsync(order, ct);
            await _db.SaveChangesAsync(ct);

            // Append domain events to Outbox
            foreach (var evt in order.DequeueDomainEvents())
                await _outbox.WriteAsync(evt, ct);

            return order;
        }

        public async Task<Order?> GetAsync(Guid id, CancellationToken ct)
        {
            return await _db.Orders.Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == id, ct);
        }

        public async Task CancelAsync(Guid id, string reason, CancellationToken ct)
        {
            var order = await _db.Orders.Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == id, ct);
            if (order == null) throw new InvalidOperationException("Order not found");

            order.Cancel(reason);
            await _db.SaveChangesAsync(ct);

            foreach (var evt in order.DequeueDomainEvents())
                await _outbox.WriteAsync(evt, ct);
        }
    }
}
