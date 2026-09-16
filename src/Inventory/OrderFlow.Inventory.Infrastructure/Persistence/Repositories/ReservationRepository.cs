using Microsoft.EntityFrameworkCore;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Entity;
using OrderFlow.Inventory.Domain.Enum;

namespace OrderFlow.Inventory.Infrastructure.Persistence.Repositories;

public sealed class ReservationRepository : IReservationRepository
{
    private readonly InventoryDbContext _dbContext;

    public ReservationRepository(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Reservation>>
        GetActiveByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reservations
            .Where(reservation =>
                reservation.OrderId == orderId &&
                reservation.Status == ReservationStatus.Active)
            .OrderBy(reservation => reservation.Sku)
            .ToListAsync(cancellationToken);
    }

    public void Add(Reservation reservation)
    {
        ArgumentNullException.ThrowIfNull(reservation);
        _dbContext.Reservations.Add(reservation);
    }
}
