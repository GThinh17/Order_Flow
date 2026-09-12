using OrderFlow.Orders.Application.Abstractions.Persistence;
using OrderFlow.Orders.Application.OrderCommand;
using OrderFlow.Orders.Domain;
using OrderFlow.Orders.Domain.Entity;
using OrderFlow.Orders.Domain.Enum;

namespace OrderFlow.Orders.Tests.Application.CreateOrder;

public sealed class CreateOrderHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidCommand_CreatesAndSavesPendingOrder()
    {
        var repository = new RecordingOrderRepository();
        var unitOfWork = new RecordingUnitOfWork();

        var utcNow = new DateTimeOffset(
            2026,
            9,
            12,
            10,
            15,
            0,
            TimeSpan.Zero);

        var timeProvider = new FixedTimeProvider(utcNow);

        var handler = new CreateOrderHandler(
            repository,
            unitOfWork,
            timeProvider);

        var command = new CreateOrderCommand(
            "cust-123",
            new[]
            {
                new CreateOrderLineCommand(
                    "WIDGET-01",
                    2,
                    10.00m),

                new CreateOrderLineCommand(
                    "WIDGET-02",
                    1,
                    5.50m)
            });

        var result = await handler.HandleAsync(command);

        Assert.NotEqual(Guid.Empty, result.OrderId);
        Assert.Equal(result.OrderId, result.CorrelationId);
        Assert.Equal(OrderStatus.Pending, result.Status);

        Assert.NotNull(repository.AddedOrder);
        Assert.Equal(result.OrderId, repository.AddedOrder.Id);
        Assert.Equal("cust-123", repository.AddedOrder.CustomerId);
        Assert.Equal(25.50m, repository.AddedOrder.TotalAmount);
        Assert.Equal(utcNow, repository.AddedOrder.CreateAt);

        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    private sealed class RecordingOrderRepository
        : IOrderRepository
    {
        public Order? AddedOrder { get; private set; }

        public void Add(Order order)
        {
            AddedOrder = order;
        }
    }

    private sealed class RecordingUnitOfWork
        : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;

            return Task.FromResult(1);
        }
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
    }
}