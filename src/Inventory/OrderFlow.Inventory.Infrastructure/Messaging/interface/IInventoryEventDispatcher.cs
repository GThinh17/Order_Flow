namespace OrderFlow.Inventory.Infrastructure.Messaging;

public interface IInventoryEventDispatcher
{
    Task DispatchAsync(
        string eventType,
        string payload,
        CancellationToken cancellationToken);
}