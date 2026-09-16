using System.Data;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Payments.Application.Abstractions.Persistence;


namespace OrderFlow.Payments.Infrastructure.Persistence
{
    public sealed class EfPaymentsTransactionRunner
        : IPaymentTransactionRunner
    {
        private readonly PaymentsDbContext _dbContext;

        public EfPaymentsTransactionRunner(PaymentsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task ExecuteAsync(
            Func<CancellationToken, Task> operation,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(operation);

            var executionStrategy =
                _dbContext.Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(
                async () =>
                {
                    _dbContext.ChangeTracker.Clear();

                    await using var transaction =
                        await _dbContext.Database.BeginTransactionAsync(
                            IsolationLevel.Serializable,
                            cancellationToken);

                    await operation(cancellationToken);

                    await _dbContext.SaveChangesAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);
                }
            );

        }
    }

}