using OrderFlow.Payments.Application.Abstractions.PaymentGateWay;
using OrderFlow.Payments.Application.Abstractions.Payments;

namespace OrderFlow.Payments.Infrastructure.PaymentGateWay
{
    public sealed class PaymentGateway : IPaymentGateWay
    {
        private const string DeclinedReason =
            "Payment was declined by the fake gateway.";

        public Task<PaymentGateWayResult> ChargeAsync(
            Guid orderId,
            decimal amount,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (orderId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Order ID is required.",
                    nameof(orderId));
            }

            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    amount,
                    "Payment amount must be greater than zero.");
            }
            var normalizedAmount = decimal.Round(
                amount,
                decimals: 2,
                MidpointRounding.AwayFromZero);

            var decimalPart =
                normalizedAmount - decimal.Truncate(normalizedAmount);

            var result = decimalPart == 0.99m
                ? PaymentGateWayResult.Failure(DeclinedReason)
                : PaymentGateWayResult.Success();

            return Task.FromResult(result);
        }
    }
}