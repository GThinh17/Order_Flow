namespace OrderFlow.Blazor.Models.Orders;

public sealed record CreateOrderRequest(
    string CustomerId,
    IReadOnlyCollection<CreateOrderLineRequest> Lines);

public sealed record CreateOrderLineRequest(
    string Sku,
    int Quantity,
    decimal UnitPrice);

public sealed record CreateOrderResponse(
    Guid OrderId,
    Guid CorrelationId,
    string Status);

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

public sealed record GetOrderLineResponse(
    string Sku,
    int Quantity,
    decimal UnitPrice);

public sealed record OrderSummaryResponse(
    Guid OrderId,
    string Status,
    decimal TotalAmount,
    DateTimeOffset CreatedAt);
