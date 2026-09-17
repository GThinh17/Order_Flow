namespace OrderFlow.Orders.Application.OrderQuery
{
    public sealed record GetOrderLineResult(
        string Sku,
        int Quantity,
        decimal UnitPrice);
}

