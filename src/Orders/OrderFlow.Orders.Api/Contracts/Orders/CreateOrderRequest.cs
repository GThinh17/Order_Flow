namespace OrderFlow.Orders.Api.Contract.Orders
{
    public sealed record class CreateOrderRequest(
        string CustomerId,
        IReadOnlyCollection<CreateOrderLineRequest> Lines
    );
}
