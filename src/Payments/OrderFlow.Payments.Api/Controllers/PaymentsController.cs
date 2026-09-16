using Microsoft.AspNetCore.Mvc;
using OrderFlow.Payments.Application.Contracts;
using OrderFlow.Payments.Application.Handler;

namespace OrderFlow.Payments.Api.Controllers;

[ApiController]
[Route("payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly GetPaymentByOrderIdHandler _handler;

    public PaymentsController(
        GetPaymentByOrderIdHandler handler)
    {
        _handler = handler;
    }

    [HttpGet("{orderId:guid}")]
    [ProducesResponseType<PaymentResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentResponse>> GetByOrderId(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var payment = await _handler.HandleAsync(
            orderId,
            cancellationToken);

        if (payment is null)
        {
            return NotFound();
        }

        return Ok(payment);
    }
}