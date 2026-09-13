namespace OrderFlow.Orders.Infrastructure.Messaging
{
    public interface IEventPublisher
    {
        ValueTask PublishAsync(
            Guid eventId,
            string eventType,
            string partitionKey,
            string payload,
            CancellationToken cancellationToken = default);
    }
}