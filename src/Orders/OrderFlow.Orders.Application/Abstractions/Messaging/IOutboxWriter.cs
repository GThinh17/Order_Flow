using OrderFlow.Contracts.IntegrationEvents.Orders;

namespace OrderFlow.Orders.Application.Abstractions.Messaging
{
    public interface IOutboxWriter
    {
        void Add(OrderPlaced orderPlaced);
    }
}