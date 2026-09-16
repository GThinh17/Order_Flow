using Microsoft.EntityFrameworkCore;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Infrastructure.Persistence.Models;

namespace OrderFlow.Inventory.Infrastructure.Persistence.Repositories;

public sealed class InboxRepository : IInboxRepository
{
    private readonly InventoryDbContext _dbContext;

    public InboxRepository(InventoryDbContext dbContext)
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
            new InboxMessage(eventId, eventType, processedAt));
    }
}
