namespace OrderFlow.Contracts.IntegrationEvents.Inventory;

public sealed record ReservationFailed(
    Guid EventId,
    Guid OrderId,
    Guid CorrelationId,
    DateTimeOffset Timestamp,
    string Reason);