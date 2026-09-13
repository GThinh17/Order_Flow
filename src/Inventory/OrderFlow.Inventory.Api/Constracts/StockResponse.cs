namespace OrderFlow.Inventory.Api.Contracts
{
    public sealed record StockResponse(
    string Sku,
    int QuantityOnHand,
    int QuantityReserved,
    int Available);
}

