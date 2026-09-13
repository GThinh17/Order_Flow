using OrderFlow.Inventory.Domain.Entity;

namespace OrderFlow.Inventory.Application.Abstractions.Persistence
{
    public interface IReservaionRepository
    {
        void Add(Reservation reservation);
    }
}