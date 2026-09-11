namespace OrderFlow.Orders.Api.Contract.Orders
{
    public sealed record CreateOrderResponse(
        Guid OrderId,
        Guid CorrelationId,
        string Status);
}