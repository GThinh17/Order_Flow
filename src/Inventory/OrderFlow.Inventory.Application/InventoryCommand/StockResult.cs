namespace OrderFlow.Inventory.Application.InventoryCommand
{
    public sealed record StockResult(
        string Sku,
        int QuantityOnHand,
        int QuantityReserved,
        int Available);
}

