using OrderFlow.Contracts.IntegrationEvents.Inventory;
using OrderFlow.Contracts.IntegrationEvents.Orders;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Application.Handler;
using OrderFlow.Inventory.Domain.Entity;
using OrderFlow.Inventory.Domain.Enum;

namespace OrderFlow.Inventory.Tests.Application
{

    public sealed class ReserveOrderHandlerTests
    {
        private static readonly DateTimeOffset UtcNow =
            new(2026, 9, 13, 12, 0, 0, TimeSpan.Zero);

        [Fact]
        public async Task HandleAsync_WhenStockIsAvailable_ReservesAndWritesSuccess()
        {
            var stock = StockItem.Create("WIDGET-01", 10);
            var stockRepository = new FakeStockRepository(stock);
            var reservations = new FakeReservationRepository();
            var inbox = new FakeInboxRepository();
            var outbox = new FakeOutboxWriter();
            var transactionRunner = new FakeTransactionRunner();
            var handler = CreateHandler(
                transactionRunner,
                stockRepository,
                reservations,
                inbox,
                outbox);
            var orderPlaced = CreateOrderPlaced(quantity: 2);

            await handler.HandleAsync(orderPlaced);

            Assert.Equal(1, transactionRunner.ExecutionCount);
            Assert.Equal(2, stock.QuantityReserved);
            Assert.Equal(8, stock.Available);
            var reservation = Assert.Single(reservations.Items);
            Assert.Equal(orderPlaced.OrderId, reservation.OrderId);
            Assert.Equal("WIDGET-01", reservation.Sku);
            Assert.Equal(2, reservation.Quantity);
            Assert.Contains(orderPlaced.EventId, inbox.EventIds);
            var succeeded = Assert.Single(outbox.Succeeded);
            Assert.Equal(orderPlaced.OrderId, succeeded.OrderId);
            Assert.Equal(orderPlaced.CorrelationId, succeeded.CorrelationId);
            Assert.Empty(outbox.Failed);
        }

        [Fact]
        public async Task HandleAsync_WhenStockIsInsufficient_WritesFailureWithoutMutation()
        {
            var stock = StockItem.Create("WIDGET-01", 1);
            var stockRepository = new FakeStockRepository(stock);
            var reservations = new FakeReservationRepository();
            var inbox = new FakeInboxRepository();
            var outbox = new FakeOutboxWriter();
            var handler = CreateHandler(
                new FakeTransactionRunner(),
                stockRepository,
                reservations,
                inbox,
                outbox);
            var orderPlaced = CreateOrderPlaced(quantity: 2);

            await handler.HandleAsync(orderPlaced);

            Assert.Equal(0, stock.QuantityReserved);
            Assert.Empty(reservations.Items);
            Assert.Empty(outbox.Succeeded);
            var failed = Assert.Single(outbox.Failed);
            Assert.Equal(orderPlaced.OrderId, failed.OrderId);
            Assert.Equal(orderPlaced.CorrelationId, failed.CorrelationId);
            Assert.Contains("Insufficient stock", failed.Reason);
            Assert.Contains(orderPlaced.EventId, inbox.EventIds);
        }

        [Fact]
        public async Task HandleAsync_WhenAnySkuIsMissing_DoesNotPartiallyReserveStock()
        {
            var availableStock = StockItem.Create("A-WIDGET", 10);
            var stockRepository = new FakeStockRepository(availableStock);
            var reservations = new FakeReservationRepository();
            var inbox = new FakeInboxRepository();
            var outbox = new FakeOutboxWriter();
            var handler = CreateHandler(
                new FakeTransactionRunner(),
                stockRepository,
                reservations,
                inbox,
                outbox);
            var orderId = Guid.NewGuid();
            var orderPlaced = new OrderPlaced(
                Guid.NewGuid(),
                orderId,
                Guid.NewGuid(),
                UtcNow,
                "customer-001",
                20m,
                [
                    new OrderPlacedLine("A-WIDGET", 1, 10m),
                new OrderPlacedLine("B-MISSING", 1, 10m)
                ]);

            await handler.HandleAsync(orderPlaced);

            Assert.Equal(0, availableStock.QuantityReserved);
            Assert.Empty(reservations.Items);
            Assert.Empty(outbox.Succeeded);
            Assert.Single(outbox.Failed);
            Assert.Contains(orderPlaced.EventId, inbox.EventIds);
        }

        [Fact]
        public async Task HandleAsync_WhenEventWasProcessed_DoesNothing()
        {
            var stock = StockItem.Create("WIDGET-01", 10);
            var stockRepository = new FakeStockRepository(stock);
            var reservations = new FakeReservationRepository();
            var inbox = new FakeInboxRepository();
            var outbox = new FakeOutboxWriter();
            var orderPlaced = CreateOrderPlaced(quantity: 2);
            inbox.Add(orderPlaced.EventId, nameof(OrderPlaced), UtcNow);
            var handler = CreateHandler(
                new FakeTransactionRunner(),
                stockRepository,
                reservations,
                inbox,
                outbox);

            await handler.HandleAsync(orderPlaced);

            Assert.Equal(0, stock.QuantityReserved);
            Assert.Empty(reservations.Items);
            Assert.Empty(outbox.Succeeded);
            Assert.Empty(outbox.Failed);
        }

        private static ReserveOrderHandler CreateHandler(
            IInventoryTransactionRunner transactionRunner,
            IStockRepository stockRepository,
            IReservationRepository reservationRepository,
            IInboxRepository inboxRepository,
            IOutboxWriter outboxWriter)
        {
            return new ReserveOrderHandler(
                transactionRunner,
                stockRepository,
                reservationRepository,
                inboxRepository,
                outboxWriter,
                new FixedTimeProvider(UtcNow));
        }

        private static OrderPlaced CreateOrderPlaced(int quantity)
        {
            var orderId = Guid.NewGuid();

            return new OrderPlaced(
                Guid.NewGuid(),
                orderId,
                Guid.NewGuid(),
                UtcNow,
                "customer-001",
                10m * quantity,
                [new OrderPlacedLine("WIDGET-01", quantity, 10m)]);
        }

        private sealed class FakeTransactionRunner : IInventoryTransactionRunner
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

        private sealed class FakeStockRepository : IStockRepository
        {
            private readonly Dictionary<string, StockItem> _items;

            public FakeStockRepository(params StockItem[] items)
            {
                _items = items.ToDictionary(item => item.Sku);
            }

            public Task<IReadOnlyList<StockItem>> GetAllAsync(
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult<IReadOnlyList<StockItem>>(
                    _items.Values.ToArray());
            }

            public Task<StockItem?> GetBySkuAsync(
                string sku,
                CancellationToken cancellationToken = default)
            {
                _items.TryGetValue(sku, out var item);
                return Task.FromResult(item);
            }

            public void Add(StockItem stockItem)
            {
                _items.Add(stockItem.Sku, stockItem);
            }
        }

        private sealed class FakeReservationRepository : IReservationRepository
        {
            public List<Reservation> Items { get; } = [];

            public Task<IReadOnlyList<Reservation>> GetActiveByOrderIdAsync(
                Guid orderId,
                CancellationToken cancellationToken = default)
            {
                IReadOnlyList<Reservation> activeReservations = Items
                    .Where(reservation =>
                        reservation.OrderId == orderId &&
                        reservation.Status == ReservationStatus.Active)
                    .ToList();

                return Task.FromResult(activeReservations);
            }

            public void Add(Reservation reservation)
            {
                Items.Add(reservation);
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

        private sealed class FakeOutboxWriter : IOutboxWriter
        {
            public List<ReservationSucceeded> Succeeded { get; } = [];
            public List<ReservationFailed> Failed { get; } = [];

            public void Add(ReservationSucceeded integrationEvent)
            {
                Succeeded.Add(integrationEvent);
            }

            public void Add(ReservationFailed integrationEvent)
            {
                Failed.Add(integrationEvent);
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
}
