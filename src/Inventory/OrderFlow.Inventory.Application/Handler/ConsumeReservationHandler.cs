using OrderFlow.Contracts.IntegrationEvents.Payments;
using OrderFlow.Inventory.Application.Abstractions.Persistence;

namespace OrderFlow.Inventory.Application.Handler
{
    public sealed class ConsumeReservationHandler
    {
        private readonly IInventoryTransactionRunner _transactionRunner;
        private readonly IReservationRepository _reservationRepository;
        private readonly IStockRepository _stockRepository;
        private readonly IInboxRepository _inboxRepository;
        private readonly TimeProvider _timeProvider;

        public ConsumeReservationHandler(
            IInventoryTransactionRunner transactionRunner,
            IReservationRepository reservationRepository,
            IStockRepository stockRepository,
            IInboxRepository inboxRepository,
            TimeProvider timeProvider)
        {
            _transactionRunner = transactionRunner;
            _reservationRepository = reservationRepository;
            _stockRepository = stockRepository;
            _inboxRepository = inboxRepository;
            _timeProvider = timeProvider;
        }

        public Task HandleAsync(
            PaymentSucceeded integrationEvent,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(integrationEvent);

            return _transactionRunner.ExecuteAsync(
                transactionToken => ProcessAsync(
                    integrationEvent,
                    transactionToken),
                cancellationToken);
        }

        private async Task ProcessAsync(
            PaymentSucceeded integrationEvent,
            CancellationToken cancellationToken)
        {
            if (await _inboxRepository.ExistsAsync(
                    integrationEvent.EventId,
                    cancellationToken))
            {
                return;
            }

            var reservations =
                await _reservationRepository
                    .GetActiveByOrderIdAsync(
                        integrationEvent.OrderId,
                        cancellationToken);

            if (reservations.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No active reservation was found for order " +
                    $"'{integrationEvent.OrderId}'.");
            }

            var utcNow = _timeProvider.GetUtcNow();

            foreach (var reservation in reservations)
            {
                var stockItem =
                    await _stockRepository.GetBySkuAsync(
                        reservation.Sku,
                        cancellationToken)
                    ?? throw new InvalidOperationException(
                        $"Stock item '{reservation.Sku}' was not found.");

                stockItem.Consume(reservation.Quantity);
                reservation.Consume(utcNow);
            }

            _inboxRepository.Add(
                integrationEvent.EventId,
                nameof(PaymentSucceeded),
                utcNow);
        }
    }
}
