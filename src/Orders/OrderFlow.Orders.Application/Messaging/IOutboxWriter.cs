using OrderFlow.Contracts.IntegrationEvents.Orders;

namespace OrderFlow.Orders.Application.Messaging
{
    public interface IOutboxWriter
    {
        void Add(OrderPlaced orderPlaced);
    }
}