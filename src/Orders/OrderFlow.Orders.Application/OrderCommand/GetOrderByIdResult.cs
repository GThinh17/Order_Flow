namespace OrderFlow.Orders.Application.OrderQuery
{
    public sealed record GetOrderByIdResult(
        Guid OrderId,
        string CustomerId,
        string Status,
        decimal TotalAmount,
        bool ReservationCompleted,
        bool PaymentCompleted,
        IReadOnlyCollection<GetOrderLineResult> Lines,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt);
}

