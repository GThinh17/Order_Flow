using OrderFlow.Inventory.Domain.Entity;

namespace OrderFlow.Inventory.Application.Abstractions.Persistence
{
    public interface IReservationRepository
    {
        void Add(Reservation reservation);
    }
}
