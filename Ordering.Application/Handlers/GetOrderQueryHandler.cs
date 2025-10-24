using MediatR;
using Ordering.Application.Interfaces;
using Ordering.Application.Models;
using Ordering.Application.Queries;

namespace Ordering.Application.Handlers
{
    public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderDto?>
    {
        private readonly IOrderService _orderService;

        public GetOrderQueryHandler(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<OrderDto?> Handle(GetOrderQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderService.GetAsync(request.OrderId, cancellationToken);
            
            if (order == null) return null;

            return new OrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Status = order.Status.ToString(),
                TotalAmount = order.Total.Amount,
                CreatedAt = DateTime.UtcNow, // Use current time or get from domain if available
                Lines = order.Lines.Select(l => new OrderLineItemDto
                {
                    Sku = l.Sku,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice.Amount,
                    LineTotal = l.Quantity * l.UnitPrice.Amount
                }).ToList()
            };
        }
    }
}
