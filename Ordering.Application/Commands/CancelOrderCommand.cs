using MediatR;

namespace Ordering.Application.Commands
{
    public record CancelOrderCommand : IRequest<bool>
    {
        public Guid OrderId { get; init; }
    }
}
