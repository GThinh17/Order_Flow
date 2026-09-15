using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Application.InventoryCommand;
using OrderFlow.Inventory.Application.Stock;

namespace OrderFlow.Inventory.Application.Handler
{

    public sealed class AdjustStockHandler
    {
        private readonly IStockRepository _stockRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AdjustStockHandler(
            IStockRepository stockRepository,
            IUnitOfWork unitOfWork)
        {
            _stockRepository = stockRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<StockResult?> HandleAsync(
            AdjustStockCommand command,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);

            if (string.IsNullOrWhiteSpace(command.Sku))
            {
                throw new ArgumentException(
                    "SKU is required.",
                    nameof(command.Sku));
            }

            var stockItem =
                await _stockRepository.GetBySkuAsync(
                    command.Sku.Trim(),
                    cancellationToken);

            if (stockItem is null)
            {
                return null;
            }

            stockItem.AdjustOnHand(command.Quantity);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            return new StockResult(
                stockItem.Sku,
                stockItem.QuantityOnHand,
                stockItem.QuantityReserved,
                stockItem.Available);
        }
    }
}
