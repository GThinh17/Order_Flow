namespace OrderFlow.Contracts.IntegrationEvents.Orders
{
    public sealed record OrderPlaced
    (
        Guid EventId,
        Guid OrderId,
        Guid CorrelationId,
        DateTimeOffset Timestamp,
        string CustomerId,
        decimal Total,
        IReadOnlyCollection<OrderPlacedLine> Lines);

    public sealed record OrderPlacedLine(
        string Sku,
        int Quantity,
        decimal UnitPrice);
}
