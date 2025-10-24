using MediatR;
using Ordering.Application.Models;

namespace Ordering.Application.Queries
{
    public record GetOrderQuery(Guid OrderId) : IRequest<OrderDto>;
}
