namespace OrderFlow.Inventory.Application.Stock
{
    public sealed record AdjustStockCommand(
        string Sku,
        int Quantity);
}

