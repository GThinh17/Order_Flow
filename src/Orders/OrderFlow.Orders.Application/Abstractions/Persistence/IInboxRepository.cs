namespace OrderFlow.Orders.Application.Abstractions.Persistence;

public interface IInboxRepository
{
    Task<bool> ExistsAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);

    void Add(
        Guid eventId,
        string eventType,
        DateTimeOffset processedAt);
}
