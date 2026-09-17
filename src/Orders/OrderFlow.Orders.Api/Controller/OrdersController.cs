using Microsoft.AspNetCore.Mvc;
using OrderFlow.Orders.Api.Contract.Orders;
using OrderFlow.Orders.Application.Handler;

namespace OrderFlow.Orders.Api.Controllers
{
    [ApiController]
    [Route("orders")]
    public sealed class OrdersController : ControllerBase
    {
        private readonly GetOrderByIdHandler _getByIdHandler;
        private readonly GetOrdersByCustomerHandler _getByCustomerHandler;

        public OrdersController(
            GetOrderByIdHandler getByIdHandler,
            GetOrdersByCustomerHandler getByCustomerHandler)
        {
            _getByIdHandler = getByIdHandler;
            _getByCustomerHandler = getByCustomerHandler;
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
            var result = await _getByIdHandler.HandleAsync(
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

        [HttpGet]
        [ProducesResponseType<IReadOnlyCollection<OrderSummaryResponse>>(
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            StatusCodes.Status400BadRequest)]
        public async Task<
            ActionResult<IReadOnlyCollection<OrderSummaryResponse>>>
            GetByCustomerIdAsync(
                [FromQuery] string? customerId,
                CancellationToken cancellationToken)
        {
            try
            {
                var results =
                    await _getByCustomerHandler.HandleAsync(
                        customerId ?? string.Empty,
                        cancellationToken);

                var response = results
                    .Select(order => new OrderSummaryResponse(
                        order.OrderId,
                        order.Status,
                        order.TotalAmount,
                        order.CreatedAt))
                    .ToArray();

                return Ok(response);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new
                {
                    error = exception.Message
                });
            }
        }
    }
}

