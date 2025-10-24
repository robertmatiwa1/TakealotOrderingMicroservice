using MediatR;
using Ordering.Domain;

namespace Ordering.Application.Commands
{
    public class CreateOrderCommand : IRequest<Order>
    {
        public Guid CustomerId { get; set; }
        public List<OrderLineItem> Lines { get; set; } = new();
    }

    public class OrderLineItem
    {
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
