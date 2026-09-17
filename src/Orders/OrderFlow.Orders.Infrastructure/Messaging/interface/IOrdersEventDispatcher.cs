namespace OrderFlow.Orders.Infrastructure.Messaging;

public interface IOrdersEventDispatcher
{
    Task DispatchAsync(
        string eventType,
        string payload,
        CancellationToken cancellationToken);
}