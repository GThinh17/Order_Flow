using OrderFlow.Inventory.Domain.Entity;

namespace OrderFlow.Inventory.Application.Abstractions.Persistence
{
    public interface IStockRepository
    {
        Task<IReadOnlyList<StockItem>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<StockItem?> GetBySkuAsync(
            string sku,
            CancellationToken cancellationToken = default);

        void Add(StockItem stockItem);
    }
}