using Microsoft.EntityFrameworkCore;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Domain.Entity;
using OrderFlow.Payments.Infrastructure.Persistence;

namespace OrderFlow.Payments.Infrastructure.Persistence.Repositories
{
    public sealed class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentsDbContext _dbContext;

        public PaymentRepository(PaymentsDbContext paymentRepository)
        {
            _dbContext = paymentRepository;
        }

        public Task<Payment?> GetByOrderIdAsync(
            Guid orderId,
            CancellationToken cancellationToken = default
        )
        {
            return _dbContext.Payments
                .AsNoTracking()
                .SingleOrDefaultAsync(payment => payment.OrderId == orderId,
                    cancellationToken);
        }

        public void Add(Payment payment)
        {
            ArgumentNullException.ThrowIfNull(payment);

            _dbContext.Payments.Add(payment);
        }
    }

}
