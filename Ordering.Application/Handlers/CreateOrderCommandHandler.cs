using MediatR;
using Ordering.Application.Commands;
using Ordering.Application.Interfaces;
using Ordering.Domain;
using Microsoft.Extensions.Logging;

namespace Ordering.Application.Handlers
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Order>
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<CreateOrderCommandHandler> _logger;

        public CreateOrderCommandHandler(IOrderService orderService, ILogger<CreateOrderCommandHandler> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        public async Task<Order> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling CreateOrderCommand for customer {CustomerId}", request.CustomerId);
            
            try
            {
                // Convert the command to service call
                var lines = request.Lines.Select(l => (sku: l.Sku, quantity: l.Quantity, unitPrice: l.UnitPrice)).ToList();
                var order = await _orderService.CreateOrderAsync(request.CustomerId, lines, cancellationToken);
                
                _logger.LogInformation("Order created with ID: {OrderId}", order.Id);
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling CreateOrderCommand for customer {CustomerId}", request.CustomerId);
                throw;
            }
        }
    }
}
