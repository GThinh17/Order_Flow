using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Application.Contracts;

namespace OrderFlow.Payments.Application.Handler
{
    public sealed class GetPaymentByOrderIdHandler
    {
        private readonly IPaymentRepository _paymentRepository;

        public GetPaymentByOrderIdHandler(
            IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<PaymentResponse?> HandleAsync(
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            var payment =
                await _paymentRepository.GetByOrderIdAsync(
                    orderId,
                    cancellationToken);

            if (payment is null)
            {
                return null;
            }

            return new PaymentResponse(
                PaymentId: payment.Id,
                OrderId: payment.OrderId,
                Amount: payment.Amount,
                Status: payment.Status.ToString(),
                CreatedAt: payment.CreatedAt);
        }
    }
}
