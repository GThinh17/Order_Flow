using OrderFlow.Orders.Domain.Entity;

namespace OrderFlow.Orders.Application.Abstractions.Persistence
{
    public interface IOrderRepository
    {
        void Add(Order order);

        Task<Order?> GetByIdAsync(
            Guid orderId,
            CancellationToken cancellationToken = default);

        Task<Order?> GetDetailsByIdAsync(
            Guid orderId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<Order>> GetByCustomerIdAsync(
            string customerId,
            CancellationToken cancellationToken = default);
    }
}
