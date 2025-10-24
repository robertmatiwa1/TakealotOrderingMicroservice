using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ordering.Domain;
using Ordering.Domain.Interfaces;
using Ordering.Infrastructure.Outbox;
using Ordering.Application.Interfaces;

namespace Ordering.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOutboxWriter _outbox;

        public OrderService(
            IOrderRepository orderRepository,
            IOutboxWriter outbox)
        {
            _orderRepository = orderRepository;
            _outbox = outbox;
        }

        public async Task<Order> CreateOrderAsync(
            Guid customerId,
            List<(string sku, int quantity, decimal unitPrice)> lines,
            CancellationToken ct)
        {
            var order = Order.Place(
                customerId,
                lines.Select(l => new OrderLine(l.sku, l.quantity, Money.Of(l.unitPrice, "ZAR"))).ToList());

            await _orderRepository.CreateAsync(order, ct);

            // Append domain events to Outbox
            foreach (var evt in order.DequeueDomainEvents())
                await _outbox.WriteAsync(evt, ct);

            return order;
        }

        public async Task<Order?> GetAsync(Guid id, CancellationToken ct)
        {
            return await _orderRepository.GetAsync(id, ct);
        }

        public async Task<bool> CancelOrderAsync(Guid orderId, CancellationToken ct)
        {
            var order = await _orderRepository.GetAsync(orderId, ct);

            if (order == null)
                return false;

            order.Cancel("Customer requested cancellation");
            await _orderRepository.UpdateAsync(order, ct);

            foreach (var evt in order.DequeueDomainEvents())
                await _outbox.WriteAsync(evt, ct);

            return true;
        }

        // Keep the existing CancelAsync for backward compatibility if needed
        public async Task CancelAsync(Guid id, string reason, CancellationToken ct)
        {
            var order = await _orderRepository.GetAsync(id, ct);

            if (order == null)
                throw new InvalidOperationException("Order not found");

            order.Cancel(reason);
            await _orderRepository.UpdateAsync(order, ct);

            foreach (var evt in order.DequeueDomainEvents())
                await _outbox.WriteAsync(evt, ct);
        }
    }
}
