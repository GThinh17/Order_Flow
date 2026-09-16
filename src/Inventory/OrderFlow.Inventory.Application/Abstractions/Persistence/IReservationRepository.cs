using OrderFlow.Inventory.Domain.Entity;

namespace OrderFlow.Inventory.Application.Abstractions.Persistence
{
    public interface IReservationRepository
    {
        Task<IReadOnlyList<Reservation>>
            GetActiveByOrderIdAsync(
                Guid orderId,
                CancellationToken cancellationToken = default);
        void Add(Reservation reservation);
    }
}
