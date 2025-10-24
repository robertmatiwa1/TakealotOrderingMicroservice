using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.Commands;
using Ordering.Application.Queries;
using Ordering.Application.Models;
using Microsoft.Extensions.Logging;

namespace Ordering.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<object>> Create([FromBody] CreateOrderCommand cmd, CancellationToken ct)
        {
            _logger.LogInformation("Received Create Order request for Customer: {CustomerId}", cmd.CustomerId);
            
            try
            {
                var order = await _mediator.Send(cmd, ct);
                _logger.LogInformation("Order created successfully with ID: {OrderId}", order.Id);
                return Ok(new { id = order.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for Customer: {CustomerId}", cmd.CustomerId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<OrderDto>> Get(Guid id, CancellationToken ct)
        {
            var dto = await _mediator.Send(new GetOrderQuery(id), ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelOrderCommand body, CancellationToken ct)
        {
            await _mediator.Send(body with { OrderId = id }, ct);
            return NoContent();
        }
    }
}
