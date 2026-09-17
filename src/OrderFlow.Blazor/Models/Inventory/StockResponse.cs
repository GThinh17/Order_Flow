namespace OrderFlow.Blazor.Models.Inventory;

public sealed record StockResponse(
    string Sku,
    int QuantityOnHand,
    int QuantityReserved,
    int Available);
