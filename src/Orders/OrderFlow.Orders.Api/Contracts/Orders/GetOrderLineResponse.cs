namespace OrderFlow.Orders.Api.Contract.Orders
{
    public sealed record GetOrderLineResponse(
    string Sku,
    int Quantity,
    decimal UnitPrice);
}

