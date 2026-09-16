namespace OrderFlow.Inventory.Infrastructure.Persistence.Messaging;

public interface IInventoryEventPublisher
{
    ValueTask PublishAsync(
        Guid eventId,
        string eventType,
        string partitionKey,
        string payload,
        CancellationToken cancellationToken = default);
}
