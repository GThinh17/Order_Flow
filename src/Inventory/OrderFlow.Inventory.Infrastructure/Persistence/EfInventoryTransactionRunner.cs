using System.Data;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Inventory.Application.Abstractions.Persistence;

namespace OrderFlow.Inventory.Infrastructure.Persistence;

public sealed class EfInventoryTransactionRunner
    : IInventoryTransactionRunner
{
    private readonly InventoryDbContext _dbContext;

    public EfInventoryTransactionRunner(
        InventoryDbContext dbContext)
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
                    await _dbContext.Database
                        .BeginTransactionAsync(
                            IsolationLevel.Serializable,
                            cancellationToken);

                await operation(cancellationToken);

                await _dbContext.SaveChangesAsync(
                    cancellationToken);

                await transaction.CommitAsync(
                    cancellationToken);
            });
    }
}