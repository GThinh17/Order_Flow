using Microsoft.AspNetCore.Mvc;
using OrderFlow.Orders.Api.Contract.Orders;
using OrderFlow.Orders.Application.Handler;

namespace OrderFlow.Orders.Api.Controllers
{
    [ApiController]
    [Route("orders")]
    public sealed class OrdersController : ControllerBase
    {
        private readonly GetOrderByIdHandler _handler;

        public OrdersController(
            GetOrderByIdHandler handler)
        {
            _handler = handler;
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType<GetOrderResponse>(
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetOrderResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _handler.HandleAsync(
                id,
                cancellationToken);

            if (result is null)
            {
                return NotFound(new
                {
                    error = $"Order '{id}' was not found."
                });
            }

            var response = new GetOrderResponse(
                result.OrderId,
                result.CustomerId,
                result.Status,
                result.TotalAmount,
                result.ReservationCompleted,
                result.PaymentCompleted,
                result.Lines
                    .Select(line => new GetOrderLineResponse(
                        line.Sku,
                        line.Quantity,
                        line.UnitPrice))
                    .ToArray(),
                result.CreatedAt,
                result.UpdatedAt);

            return Ok(response);
        }
    }
}

