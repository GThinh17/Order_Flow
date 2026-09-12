namespace OrderFlow.Orders.Application.OrderCommand
{
    public sealed record CreateOrderLineCommand(
        string Sku,
        int Quantity,
        decimal UnitPrice
    );
}