namespace OrderFlow.Orders.Application.Abstractions.Persistence;

public interface IOrdersTransactionRunner
{
    Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);
}
