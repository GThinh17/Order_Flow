using OrderFlow.Contracts.IntegrationEvents.Orders;
using OrderFlow.Orders.Application.Handler;
using OrderFlow.Orders.Application.Abstractions.Persistence;
using OrderFlow.Orders.Application.OrderCommand;
using OrderFlow.Orders.Domain.Entity;
using OrderFlow.Orders.Domain.Enum;

namespace OrderFlow.Orders.Tests.Application.CreateOrder
{

    public sealed class CreateOrderHandlerTests
    {
        [Fact]
        public async Task HandleAsync_WithValidCommand_CreatesOrderAndOutboxMessageThenSavesOnce()
        {
            var repository = new RecordingOrderRepository();
            var sagaStateRepository =
                new RecordingOrderSagaStateRepository();
            var unitOfWork = new RecordingUnitOfWork();
            var outboxWriter = new RecordingOutboxWriter();

            var utcNow = new DateTimeOffset(
                2026,
                9,
                13,
                10,
                15,
                0,
                TimeSpan.Zero);

            var handler = new CreateOrderHandler(
                repository,
                sagaStateRepository,
                unitOfWork,
                outboxWriter,
                new FixedTimeProvider(utcNow));

            var command = new CreateOrderCommand(
                "cust-123",
                [
                    new CreateOrderLineCommand(
                    "WIDGET-01",
                    2,
                    10.00m),
                new CreateOrderLineCommand(
                    "WIDGET-02",
                    1,
                    5.50m)
                ]);

            var result = await handler.HandleAsync(command);

            Assert.NotEqual(Guid.Empty, result.OrderId);
            Assert.Equal(result.OrderId, result.CorrelationId);
            Assert.Equal(OrderStatus.Pending, result.Status);

            var addedOrder = Assert.IsType<Order>(repository.AddedOrder);
            Assert.Equal(result.OrderId, addedOrder.Id);
            Assert.Equal("cust-123", addedOrder.CustomerId);
            Assert.Equal(25.50m, addedOrder.TotalAmount);
            Assert.Equal(utcNow, addedOrder.CreatedAt);
            Assert.Equal(utcNow, addedOrder.UpdatedAt);

            var addedSagaState = Assert.IsType<OrderSagaState>(
                sagaStateRepository.AddedSagaState);
            Assert.Equal(result.OrderId, addedSagaState.OrderId);
            Assert.False(addedSagaState.ReservationCompleted);
            Assert.False(addedSagaState.PaymentCompleted);

            var addedEvent = Assert.IsType<OrderPlaced>(
                outboxWriter.AddedEvent);
            Assert.Equal(result.OrderId, addedEvent.OrderId);
            Assert.Equal(result.CorrelationId, addedEvent.CorrelationId);
            Assert.Equal(utcNow, addedEvent.Timestamp);
            Assert.Equal(2, addedEvent.Lines.Count);

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

            public Task<Order?> GetByIdAsync(
                Guid orderId,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(
                    AddedOrder?.Id == orderId
                        ? AddedOrder
                        : null);
            }
        }

        private sealed class RecordingOrderSagaStateRepository
            : IOrderSagaStateRepository
        {
            public OrderSagaState? AddedSagaState { get; private set; }

            public void Add(OrderSagaState sagaState)
            {
                AddedSagaState = sagaState;
            }

            public Task<OrderSagaState?> GetByOrderIdAsync(
                Guid orderId,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(
                    AddedSagaState?.OrderId == orderId
                        ? AddedSagaState
                        : null);
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

        private sealed class RecordingOutboxWriter
            : IOutboxWriter
        {
            public OrderPlaced? AddedEvent { get; private set; }

            public void Add(OrderPlaced orderPlaced)
            {
                AddedEvent = orderPlaced;
            }
        }

        private sealed class FixedTimeProvider(
            DateTimeOffset utcNow)
            : TimeProvider
        {
            public override DateTimeOffset GetUtcNow()
            {
                return utcNow;
            }
        }
    }

}
