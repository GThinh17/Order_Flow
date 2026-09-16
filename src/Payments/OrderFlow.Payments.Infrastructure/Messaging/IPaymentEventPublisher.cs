namespace OrderFlow.Payments.Infrastructure.Persistence.Messaging;

public interface IPaymentEventPublisher
{
    ValueTask PublishAsync(
        Guid eventId,
        string eventType,
        string partitionKey,
        string payload,
        CancellationToken cancellationToken = default);
}
