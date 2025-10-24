using MediatR;
using Ordering.Application.Commands;
using Ordering.Application.Interfaces;

namespace Ordering.Application.Handlers
{
    public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, bool>
    {
        private readonly IOrderService _orderService;

        public CancelOrderCommandHandler(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            return await _orderService.CancelOrderAsync(request.OrderId, cancellationToken);
        }
    }
}
