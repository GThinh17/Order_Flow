using OrderFlow.Orders.Domain.Entity;

namespace OrderFlow.Orders.Application.Abstractions.Persistence;

public interface IOrderSagaStateRepository
{
    void Add(OrderSagaState sagaState);

    Task<OrderSagaState?> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);
}
