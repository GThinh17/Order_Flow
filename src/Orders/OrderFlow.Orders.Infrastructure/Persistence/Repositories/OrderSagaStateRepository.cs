using Microsoft.EntityFrameworkCore;
using OrderFlow.Orders.Application.Abstractions.Persistence;
using OrderFlow.Orders.Domain.Entity;

namespace OrderFlow.Orders.Infrastructure.Persistence.Repositories;

public sealed class OrderSagaStateRepository
    : IOrderSagaStateRepository
{
    private readonly OrdersDbContext _dbContext;

    public OrderSagaStateRepository(
        OrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(OrderSagaState sagaState)
    {
        _dbContext.OrderSagaStates.Add(sagaState);
    }

    public Task<OrderSagaState?> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.OrderSagaStates
            .SingleOrDefaultAsync(
                state => state.OrderId == orderId,
                cancellationToken);
    }
}
