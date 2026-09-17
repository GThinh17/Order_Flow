using Microsoft.EntityFrameworkCore;
using OrderFlow.Orders.Application.Abstractions.Persistence;
using OrderFlow.Orders.Domain.Entity;

namespace OrderFlow.Orders.Infrastructure.Persistence.Repositories
{
    public sealed class OrderRepository
        : IOrderRepository
    {
        private readonly OrdersDbContext _dbContext;

        public OrderRepository(
            OrdersDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(Order order)
        {
            _dbContext.Orders.Add(order);
        }

        public Task<Order?> GetByIdAsync(
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            return _dbContext.Orders
                .SingleOrDefaultAsync(
                    order => order.Id == orderId,
                    cancellationToken);
        }
        public Task<Order?> GetDetailsByIdAsync(
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            return _dbContext.Orders
                .AsNoTracking()
                .Include(order => order.Lines)
                .SingleOrDefaultAsync(
                    order => order.Id == orderId,
                    cancellationToken);
        }
    }
}
