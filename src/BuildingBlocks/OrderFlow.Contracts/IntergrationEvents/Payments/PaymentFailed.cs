namespace OrderFlow.Contracts.IntegrationEvents.Payments
{
    public sealed record PaymentFailed(
        Guid EventId,
        Guid OrderId,
        Guid CorrelationId,
        DateTimeOffset Timestamp,
        Guid PaymentId,
        decimal Amount,
        string Reason);
}
