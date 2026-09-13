using Microsoft.EntityFrameworkCore;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Entity;

namespace OrderFlow.Inventory.Infrastructure.Persistence.Repositories;

public sealed class StockRepository
    : IStockRepository
{
    private readonly InventoryDbContext _dbContext;

    public StockRepository(
        InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<StockItem>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.StockItems
            .AsNoTracking()
            .OrderBy(stock => stock.Sku)
            .ToListAsync(cancellationToken);
    }

    public Task<StockItem?> GetBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.StockItems
            .SingleOrDefaultAsync(
                stock => stock.Sku == sku,
                cancellationToken);
    }

    public void Add(StockItem stockItem)
    {
        _dbContext.StockItems.Add(stockItem);
    }
}