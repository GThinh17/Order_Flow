namespace OrderFlow.Orders.Api.Contract.Orders
{
    public sealed record class CreateOrderLineRequest(
        string Sku,
        int Quantity,
        decimal UnitPrice);
}