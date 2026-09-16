using OrderFlow.Payments.Domain.Entity;

namespace OrderFlow.Payments.Application.Abstractions.Persistence
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByOrderIdAsync(
            Guid orderId,
            CancellationToken cancellationToken = default);

        void Add(Payment payment);
    }
}
