using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Application.InventoryCommand;

namespace OrderFlow.Inventory.Application.InventoryCommand;

public sealed class GetStockHandler
{
    private readonly IStockRepository _stockRepository;

    public GetStockHandler(
        IStockRepository stockRepository)
    {
        _stockRepository = stockRepository;
    }

    public async Task<IReadOnlyList<StockResult>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var stockItems =
            await _stockRepository.GetAllAsync(
                cancellationToken);

        return stockItems
            .Select(stock => new StockResult(
                stock.Sku,
                stock.QuantityOnHand,
                stock.QuantityReserved,
                stock.Available))
            .ToArray();
    }
}