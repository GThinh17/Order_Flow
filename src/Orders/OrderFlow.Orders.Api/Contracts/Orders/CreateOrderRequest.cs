namespace OrderFlow.Orders.Api.Contract.Orders
{
    public sealed record class CreateOrderRequest(
        Guid OrderId,
        Guid Correlation,
        string Status
    );
}
