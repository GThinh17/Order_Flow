using OrderFlow.Payments.Application.Abstractions.PaymentGateWay;

namespace OrderFlow.Payments.Application.Abstractions.Payments
{
    public interface IPaymentGateWay
    {
        Task<PaymentGateWayResult> ChargeAsync(
            Guid orderId,
            decimal amount,
            CancellationToken cancellationToken = default);
    }

}