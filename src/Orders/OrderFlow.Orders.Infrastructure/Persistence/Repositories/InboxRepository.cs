using Microsoft.EntityFrameworkCore;
using OrderFlow.Orders.Application.Abstractions.Persistence;

namespace OrderFlow.Orders.Infrastructure.Persistence.Repositories;

public sealed class InboxRepository : IInboxRepository
{
    private readonly OrdersDbContext _dbContext;

    public InboxRepository(OrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.InboxMessages
            .AsNoTracking()
            .AnyAsync(
                message => message.EventId == eventId,
                cancellationToken);
    }

    public void Add(
        Guid eventId,
        string eventType,
        DateTimeOffset processedAt)
    {
        _dbContext.InboxMessages.Add(
            new InboxMessage(
                eventId,
                eventType,
                processedAt));
    }
}
