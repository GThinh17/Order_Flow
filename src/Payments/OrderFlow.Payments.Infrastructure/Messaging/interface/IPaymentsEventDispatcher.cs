namespace OrderFlow.Payments.Infrastructure.Messaging
{
    public interface IPaymentsEventDispatcher
    {
        Task DispatchAsync(
            string eventType,
            string payload,
            CancellationToken cancellationToken);
    }
}
