namespace OrderFlow.Inventory.Application.Abstractions.Persistence
{
    public interface IInventoryTransactionRunner
    {
        Task ExecuteAsync(
            Func<CancellationToken, Task> operation,
            CancellationToken cancellationToken = default);
    }
}