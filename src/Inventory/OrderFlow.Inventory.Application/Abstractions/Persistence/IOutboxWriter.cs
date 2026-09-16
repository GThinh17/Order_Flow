using OrderFlow.Contracts.IntegrationEvents.Inventory;

namespace OrderFlow.Inventory.Application.Abstractions.Persistence
{
    public interface IOutboxWriter
    {
        void Add(ReservationSucceeded integrationEvent);

        void Add(ReservationFailed integrationEvent);
    }
}
