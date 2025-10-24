using MediatR;
using Ordering.Application.Models;

namespace Ordering.Application.Queries
{
    public record GetOrderQuery(Guid Id) : IRequest<OrderDto?>;
}
