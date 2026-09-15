using OrderFlow.Contracts.IntegrationEvents.Inventory;
using OrderFlow.Orders.Application.Abstractions.Persistence;
using OrderFlow.Orders.Application.Handler;
using OrderFlow.Orders.Domain.Entity;
using OrderFlow.Orders.Domain.Enum;

namespace OrderFlow.Orders.Tests.Application.Reservation
{

    public sealed class ReservationHandlersTests
    {
        private static readonly DateTimeOffset UtcNow =
            new(2026, 9, 15, 12, 0, 0, TimeSpan.Zero);

        [Fact]
        public async Task ReservationSucceeded_MovesOrderToChargingAndCompletesSaga()
        {
            var order = CreateReservingOrder();
            var sagaState = OrderSagaState.Create(order.Id);
            var inbox = new FakeInboxRepository();
            var transaction = new FakeTransactionRunner();
            var integrationEvent = CreateReservationSucceeded(order.Id);
            var handler = new ReservationSucceededHandler(
                transaction,
                new FakeOrderRepository(order),
                new FakeSagaStateRepository(sagaState),
                inbox,
                new FixedTimeProvider(UtcNow));

            await handler.HandleAsync(integrationEvent);

            Assert.Equal(OrderStatus.Charging, order.Status);
            Assert.True(sagaState.ReservationCompleted);
            Assert.Equal(
                integrationEvent.EventId,
                sagaState.LastProcessedEventId);
            Assert.Contains(integrationEvent.EventId, inbox.EventIds);
            Assert.Equal(1, transaction.ExecutionCount);
        }

        [Fact]
        public async Task ReservationFailed_MovesOrderToCancelledAndCompletesSaga()
        {
            var order = CreateReservingOrder();
            var sagaState = OrderSagaState.Create(order.Id);
            var inbox = new FakeInboxRepository();
            var integrationEvent = new ReservationFailed(
                Guid.NewGuid(),
                order.Id,
                order.Id,
                UtcNow,
                "Insufficient stock.");
            var handler = new ReservationFailedHandler(
                new FakeTransactionRunner(),
                new FakeOrderRepository(order),
                new FakeSagaStateRepository(sagaState),
                inbox,
                new FixedTimeProvider(UtcNow));

            await handler.HandleAsync(integrationEvent);

            Assert.Equal(OrderStatus.Cancelled, order.Status);
            Assert.True(sagaState.ReservationCompleted);
            Assert.False(sagaState.PaymentCompleted);
            Assert.Contains(integrationEvent.EventId, inbox.EventIds);
        }

        [Fact]
        public async Task DuplicateReservationEvent_DoesNotChangeOrderAgain()
        {
            var order = CreateReservingOrder();
            var sagaState = OrderSagaState.Create(order.Id);
            var inbox = new FakeInboxRepository();
            var integrationEvent = CreateReservationSucceeded(order.Id);
            inbox.Add(
                integrationEvent.EventId,
                nameof(ReservationSucceeded),
                UtcNow);
            var handler = new ReservationSucceededHandler(
                new FakeTransactionRunner(),
                new FakeOrderRepository(order),
                new FakeSagaStateRepository(sagaState),
                inbox,
                new FixedTimeProvider(UtcNow));

            await handler.HandleAsync(integrationEvent);

            Assert.Equal(OrderStatus.Reserving, order.Status);
            Assert.False(sagaState.ReservationCompleted);
            Assert.Single(inbox.EventIds);
        }

        [Fact]
        public async Task ReservationSucceeded_WhenOrderIsMissing_Throws()
        {
            var orderId = Guid.NewGuid();
            var handler = new ReservationSucceededHandler(
                new FakeTransactionRunner(),
                new FakeOrderRepository(),
                new FakeSagaStateRepository(
                    OrderSagaState.Create(orderId)),
                new FakeInboxRepository(),
                new FixedTimeProvider(UtcNow));

            var action = () => handler.HandleAsync(
                CreateReservationSucceeded(orderId));

            await Assert.ThrowsAsync<InvalidOperationException>(action);
        }

        private static Order CreateReservingOrder()
        {
            var order = Order.Create(
                "cust-123",
                [OrderLine.Create("WIDGET-01", 2, 10m)],
                UtcNow.AddMinutes(-1));

            order.StartReserving(UtcNow.AddSeconds(-1));
            return order;
        }

        private static ReservationSucceeded CreateReservationSucceeded(
            Guid orderId)
        {
            return new ReservationSucceeded(
                Guid.NewGuid(),
                orderId,
                orderId,
                UtcNow,
                Guid.NewGuid(),
                [new ReservedLine("WIDGET-01", 2)]);
        }

        private sealed class FakeTransactionRunner
            : IOrdersTransactionRunner
        {
            public int ExecutionCount { get; private set; }

            public async Task ExecuteAsync(
                Func<CancellationToken, Task> operation,
                CancellationToken cancellationToken = default)
            {
                ExecutionCount++;
                await operation(cancellationToken);
            }
        }

        private sealed class FakeOrderRepository(params Order[] orders)
            : IOrderRepository
        {
            private readonly Dictionary<Guid, Order> _orders =
                orders.ToDictionary(order => order.Id);

            public void Add(Order order)
            {
                _orders.Add(order.Id, order);
            }

            public Task<Order?> GetByIdAsync(
                Guid orderId,
                CancellationToken cancellationToken = default)
            {
                _orders.TryGetValue(orderId, out var order);
                return Task.FromResult(order);
            }
        }

        private sealed class FakeSagaStateRepository(
            params OrderSagaState[] states)
            : IOrderSagaStateRepository
        {
            private readonly Dictionary<Guid, OrderSagaState> _states =
                states.ToDictionary(state => state.OrderId);

            public void Add(OrderSagaState sagaState)
            {
                _states.Add(sagaState.OrderId, sagaState);
            }

            public Task<OrderSagaState?> GetByOrderIdAsync(
                Guid orderId,
                CancellationToken cancellationToken = default)
            {
                _states.TryGetValue(orderId, out var state);
                return Task.FromResult(state);
            }
        }

        private sealed class FakeInboxRepository : IInboxRepository
        {
            public HashSet<Guid> EventIds { get; } = [];

            public Task<bool> ExistsAsync(
                Guid eventId,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(EventIds.Contains(eventId));
            }

            public void Add(
                Guid eventId,
                string eventType,
                DateTimeOffset processedAt)
            {
                EventIds.Add(eventId);
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
