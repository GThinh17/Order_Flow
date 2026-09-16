namespace OrderFlow.Contracts.IntegrationEvents.Inventory
{
    public sealed record ReservationSucceeded(
        Guid EventId,
        Guid OrderId,
        Guid CorrelationId,
        DateTimeOffset Timestamp,
        Guid ReservationId,
        decimal Total,
        IReadOnlyCollection<ReservedLine> Lines);
}