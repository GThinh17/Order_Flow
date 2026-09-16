

namespace OrderFlow.Contracts.IntegrationEvents.Payments
{
    public sealed record PaymentSucceeded(
        Guid EventId,
        Guid OrderId,
        Guid CorrelationId,
        DateTimeOffset Timestamp,
        Guid PaymentId,
        decimal Amount);
}