namespace OrderFlow.Orders.Application.OrderQuery;

public sealed record GetOrderSummaryResult(
    Guid OrderId,
    string Status,
    decimal TotalAmount,
    DateTimeOffset CreatedAt);
