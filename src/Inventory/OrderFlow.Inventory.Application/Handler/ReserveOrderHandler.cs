using OrderFlow.Contracts.IntegrationEvents.Inventory;
using OrderFlow.Contracts.IntegrationEvents.Orders;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Entity;

namespace OrderFlow.Inventory.Application.Handler
{
    public sealed class ReserveOrderHandler
    {
        private readonly IInventoryTransactionRunner
            _inventoryTransactionRunner;
        private readonly IStockRepository
            _stockRepository;
        private readonly IReservationRepository
            _reservationRepository;
        private readonly IInboxRepository
            _inboxRepository;
        private readonly IInventoryOutboxWriter
            _inventoryOutboxWriter;
        private readonly TimeProvider
            _timeProvider;

        public ReserveOrderHandler(
            IInventoryTransactionRunner inventoryTransactionRunner,
            IStockRepository stockRepository,
            IReservationRepository reservationRepository,
            IInboxRepository inboxRepository,
            IInventoryOutboxWriter inventoryOutboxWriter,
            TimeProvider timeProvider)
        {
            _inventoryTransactionRunner = inventoryTransactionRunner;
            _stockRepository = stockRepository;
            _reservationRepository = reservationRepository;
            _inboxRepository = inboxRepository;
            _inventoryOutboxWriter = inventoryOutboxWriter;
            _timeProvider = timeProvider;
        }

        public Task HandleAsync(
            OrderPlaced orderPlaced,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(orderPlaced);
            ArgumentNullException.ThrowIfNull(orderPlaced.Lines);

            return _inventoryTransactionRunner.ExecuteAsync(
                transactionToken =>
                    ProcessAsync(
                        orderPlaced,
                        transactionToken),
                cancellationToken);
        }
        private async Task ProcessAsync(
            OrderPlaced orderPlaced,
            CancellationToken cancellationToken)
        {
            var alreadyProcessed =
                await _inboxRepository.ExistsAsync(
                    orderPlaced.EventId,
                    cancellationToken);

            if (alreadyProcessed)
            {
                return;
            }

            var requestedLines = orderPlaced.Lines
                .GroupBy(
                    line => line.Sku.Trim(),
                    StringComparer.Ordinal)
                .Select(group => new RequestedLine(
                    group.Key,
                    group.Sum(line => line.Quantity)))
                .OrderBy(line => line.Sku)
                .ToArray();

            if (requestedLines.Length == 0)
            {
                throw new ArgumentException(
                    "OrderPlaced must contain at least one line.");
            }

            var stockBySku =
                new Dictionary<string, StockItem>(
                    StringComparer.Ordinal);

            string? failureReason = null;

            foreach (var line in requestedLines)
            {
                var stockItem =
                    await _stockRepository.GetBySkuAsync(
                        line.Sku,
                        cancellationToken);

                if (stockItem is null)
                {
                    failureReason =
                        $"SKU '{line.Sku}' does not exist.";

                    break;
                }

                if (stockItem.Available < line.Quantity)
                {
                    failureReason =
                        $"Insufficient stock for SKU '{line.Sku}'.";

                    break;
                }

                stockBySku.Add(
                    line.Sku,
                    stockItem);
            }

            var utcNow = _timeProvider.GetUtcNow();

            if (failureReason is not null)
            {
                var failedEvent =
                    new ReservationFailed(
                        Guid.NewGuid(),
                        orderPlaced.OrderId,
                        orderPlaced.CorrelationId,
                        utcNow,
                        failureReason);

                _inventoryOutboxWriter.Add(failedEvent);
            }
            else
            {
                CreateSuccessfulReservation(
                    orderPlaced,
                    requestedLines,
                    stockBySku,
                    utcNow);
            }

            _inboxRepository.Add(
                orderPlaced.EventId,
                nameof(OrderPlaced),
                utcNow);
        }

        private void CreateSuccessfulReservation(
            OrderPlaced orderPlaced,
            IReadOnlyCollection<RequestedLine> requestedLines,
            IReadOnlyDictionary<string, StockItem> stockBySku,
            DateTimeOffset utcNow)
        {
            var reservationId = Guid.NewGuid();

            foreach (var line in requestedLines)
            {
                var stockItem = stockBySku[line.Sku];

                if (!stockItem.TryReserve(line.Quantity))
                {
                    throw new InvalidOperationException(
                        $"Stock invariant failed for SKU '{line.Sku}'.");
                }

                var reservation =
                    Reservation.CreateActive(
                        reservationId,
                        orderPlaced.OrderId,
                        line.Sku,
                        line.Quantity,
                        utcNow);

                _reservationRepository.Add(reservation);
            }

            var succeededEvent =
                new ReservationSucceeded(
                    Guid.NewGuid(),
                    orderPlaced.OrderId,
                    orderPlaced.CorrelationId,
                    utcNow,
                    reservationId,
                    requestedLines
                        .Select(line => new ReservedLine(
                            line.Sku,
                            line.Quantity))
                        .ToArray());

            _inventoryOutboxWriter.Add(succeededEvent);
        }
        private sealed record RequestedLine(
            string Sku,
            int Quantity);
    }
}

