namespace OrderFlow.Orders.Api.Contract.Orders;

public sealed record OrderSummaryResponse(
    Guid OrderId,
    string Status,
    decimal TotalAmount,
    DateTimeOffset CreatedAt);
