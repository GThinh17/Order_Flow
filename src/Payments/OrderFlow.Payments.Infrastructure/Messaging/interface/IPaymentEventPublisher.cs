namespace OrderFlow.Payments.Infrastructure.Messaging;

public interface IPaymentEventPublisher
{
    ValueTask PublishAsync(
        Guid eventId,
        string eventType,
        string partitionKey,
        string payload,
        CancellationToken cancellationToken = default);
}
