using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Entity;

namespace OrderFlow.Inventory.Infrastructure.Persistence.Repositories;

public sealed class ReservationRepository : IReservationRepository
{
    private readonly InventoryDbContext _dbContext;

    public ReservationRepository(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(Reservation reservation)
    {
        ArgumentNullException.ThrowIfNull(reservation);
        _dbContext.Reservations.Add(reservation);
    }
}
