namespace OrderFlow.Orders.Api.Contract.Orders
{
    public sealed record GetOrderResponse(
    Guid OrderId,
    string CustomerId,
    string Status,
    decimal TotalAmount,
    bool ReservationCompleted,
    bool PaymentCompleted,
    IReadOnlyCollection<GetOrderLineResponse> Lines,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
}

