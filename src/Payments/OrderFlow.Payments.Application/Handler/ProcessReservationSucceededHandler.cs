using OrderFlow.Contracts.IntegrationEvents.Inventory;
using OrderFlow.Contracts.IntegrationEvents.Payments;
using OrderFlow.Payments.Application.Abstractions.PaymentGateWay;
using OrderFlow.Payments.Application.Abstractions.Payments;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Domain.Entity;

namespace OrderFlow.Payments.Application.Handler
{
    public sealed class ProcessReservationSucceededHandler
    {
        private readonly IPaymentGateWay _paymentGateway;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IInboxRepository _inboxRepository;
        private readonly IOutboxWriter _outboxWriter;
        private readonly IPaymentTransactionRunner _transactionRunner;
        private readonly TimeProvider _timeProvider;

        public ProcessReservationSucceededHandler(
            IPaymentGateWay paymentGateway,
            IPaymentRepository paymentRepository,
            IInboxRepository inboxRepository,
            IOutboxWriter outboxWriter,
            IPaymentTransactionRunner transactionRunner,
            TimeProvider timeProvider)
        {
            _paymentGateway = paymentGateway;
            _paymentRepository = paymentRepository;
            _inboxRepository = inboxRepository;
            _outboxWriter = outboxWriter;
            _transactionRunner = transactionRunner;
            _timeProvider = timeProvider;
        }

        public async Task HandleAsync(
            ReservationSucceeded integrationEvent,
            CancellationToken cancellationToken = default
        )
        {
            ArgumentNullException.ThrowIfNull(integrationEvent);

            await _transactionRunner.ExecuteAsync(
                async transactionCancellationToken =>
                {
                    var eventAlreadyProceed =
                        await _inboxRepository.ExistAsync(
                            integrationEvent.EventId,
                            transactionCancellationToken);

                    if (eventAlreadyProceed)
                    {
                        return;
                    }

                    var existingPayment =
                        await _paymentRepository.GetByOrderIdAsync(
                            integrationEvent.OrderId,
                            transactionCancellationToken);

                    var utc = _timeProvider.GetUtcNow();

                    if (existingPayment is not null)
                    {
                        _inboxRepository.Add(
                            integrationEvent.EventId,
                            nameof(ReservationSucceeded),
                            utc);

                        return;
                    }

                    var gatewayResult = await _paymentGateway.ChargeAsync(
                        integrationEvent.OrderId,
                        integrationEvent.Total,
                        transactionCancellationToken
                    );

                    if (gatewayResult.IsSuccessful)
                    {
                        ProcessSuccessfulPayment(
                            integrationEvent,
                            utc);
                    }
                    else
                    {
                        ProcessFailedPayment(
                            integrationEvent,
                            gatewayResult,
                            utc);
                    }

                    _inboxRepository.Add(
                        integrationEvent.EventId,
                        nameof(ReservationSucceeded),
                        utc);
                },
                cancellationToken
            );
        }
        private void ProcessSuccessfulPayment(
       ReservationSucceeded integrationEvent,
       DateTimeOffset utcNow)
        {
            var payment = Payment.CreateSucceeded(
                integrationEvent.OrderId,
                integrationEvent.Total,
                utcNow);

            _paymentRepository.Add(payment);

            var paymentSucceeded = new PaymentSucceeded(
                EventId: Guid.NewGuid(),
                OrderId: integrationEvent.OrderId,
                CorrelationId: integrationEvent.CorrelationId,
                Timestamp: utcNow,
                PaymentId: payment.Id,
                Amount: payment.Amount);

            _outboxWriter.Add(paymentSucceeded);
        }

        private void ProcessFailedPayment(
            ReservationSucceeded integrationEvent,
            PaymentGateWayResult gatewayResult,
            DateTimeOffset utcNow)
        {
            var failureReason =
                gatewayResult.FailureReason
                ?? "Payment failed for an unknown reason.";

            var payment = Payment.CreateFailed(
                integrationEvent.OrderId,
                integrationEvent.Total,
                failureReason,
                utcNow);

            _paymentRepository.Add(payment);

            var paymentFailed = new PaymentFailed(
                EventId: Guid.NewGuid(),
                OrderId: integrationEvent.OrderId,
                CorrelationId: integrationEvent.CorrelationId,
                Timestamp: utcNow,
                PaymentId: payment.Id,
                Amount: payment.Amount,
                Reason: failureReason);

            _outboxWriter.Add(paymentFailed);
        }
    }
}