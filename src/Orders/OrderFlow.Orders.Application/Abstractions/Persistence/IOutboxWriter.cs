using OrderFlow.Contracts.IntegrationEvents.Orders;

namespace OrderFlow.Orders.Application.Abstractions.Persistence
{
    public interface IOutboxWriter
    {
        void Add(OrderPlaced orderPlaced);
    }
}
