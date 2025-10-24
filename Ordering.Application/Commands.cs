using MediatR;
using Ordering.Application.Models;

namespace Ordering.Application.Commands
{
    public record CreateOrderCommand(Guid CustomerId, List<OrderLineDto> Lines) : IRequest<Guid>;

    public record CancelOrderCommand(Guid OrderId, string Reason) : IRequest;
}
