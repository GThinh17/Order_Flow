using OrderFlow.Orders.Domain.Entity;

namespace OrderFlow.Orders.Application.Abstractions.Persistence
{
    public interface IOrderRepository
    {
        void Add(Order order);
    }
}