using Microsoft.AspNetCore.Mvc;
using Ordering.Application.Interfaces;
using Ordering.Api.Contracts.Requests;
using Ordering.Api.Contracts.Responses;

namespace Ordering.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<ActionResult<OrderResponse>> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
        {
            var result = await _orderService.CreateOrderAsync(request.CustomerId, request.Lines.Select(l => (l.Sku, l.Quantity, l.UnitPrice)).ToList(), ct);
            return Ok(new OrderResponse
            {
                Id = result.Id.ToString(),
                Status = result.Status.ToString(),
                CustomerId = result.CustomerId.ToString(),
                Total = result.Total.Amount,
                Currency = result.Total.Currency
            });
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<OrderResponse>> Get(Guid id, CancellationToken ct)
        {
            var result = await _orderService.GetAsync(id, ct);
            if (result == null) return NotFound();
            return Ok(new OrderResponse
            {
                Id = result.Id.ToString(),
                Status = result.Status.ToString(),
                CustomerId = result.CustomerId.ToString(),
                Total = result.Total.Amount,
                Currency = result.Total.Currency
            });
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelOrderRequest request, CancellationToken ct)
        {
            await _orderService.CancelAsync(id, request.Reason ?? "Cancelled by user", ct);
            return NoContent();
        }
    }
}
