namespace OrderFlow.Payments.Application.Abstractions.Persistence
{
    public interface IPaymentTransactionRunner
    {
        Task ExecuteAsync(
            Func<CancellationToken, Task> operation,
            CancellationToken cancellationToken = default);
    }
}
