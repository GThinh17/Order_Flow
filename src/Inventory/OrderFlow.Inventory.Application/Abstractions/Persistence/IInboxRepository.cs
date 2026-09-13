namespace OrderFlow.Inventory.Application.Abstractions.Persistence
{
    public interface IInboxRepository
    {
        Task<bool> ExistsAsync(
            Guid eventId,
            CancellationToken cancellationToken);

        void Add(
            Guid eventId,
            string eventType,
            DateTimeOffset procssedAt);
    }
}