namespace OrderFlow.Payments.Application.Abstractions.Persistence
{
    public interface IInboxRepository
    {
        Task<bool> ExistAsync(
            Guid eventId,
            CancellationToken cancellationToken = default);

        void Add(
            Guid eventId,
            string eventType,
            DateTimeOffset processedAt);
    }
}