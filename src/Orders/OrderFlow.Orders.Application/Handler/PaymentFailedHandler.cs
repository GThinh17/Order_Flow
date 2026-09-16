using OrderFlow.Contracts.IntegrationEvents.Payments;
using OrderFlow.Orders.Application.Abstractions.Persistence;

namespace OrderFlow.Orders.Application.Handler;

public sealed class PaymentFailedHandler
{
    private readonly IOrdersTransactionRunner _transactionRunner;
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderSagaStateRepository _sagaStateRepository;
    private readonly IInboxRepository _inboxRepository;
    private readonly TimeProvider _timeProvider;

    public PaymentFailedHandler(
        IOrdersTransactionRunner transactionRunner,
        IOrderRepository orderRepository,
        IOrderSagaStateRepository sagaStateRepository,
        IInboxRepository inboxRepository,
        TimeProvider timeProvider)
    {
        _transactionRunner = transactionRunner;
        _orderRepository = orderRepository;
        _sagaStateRepository = sagaStateRepository;
        _inboxRepository = inboxRepository;
        _timeProvider = timeProvider;
    }

    public Task HandleAsync(
        PaymentFailed integrationEvent,
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
        PaymentFailed integrationEvent,
        CancellationToken cancellationToken)
    {
        if (await _inboxRepository.ExistsAsync(
                integrationEvent.EventId,
                cancellationToken))
        {
            return;
        }

        var order = await _orderRepository.GetByIdAsync(
                integrationEvent.OrderId,
                cancellationToken)
            ?? throw new InvalidOperationException(
                $"Order '{integrationEvent.OrderId}' was not found.");

        var sagaState =
            await _sagaStateRepository.GetByOrderIdAsync(
                integrationEvent.OrderId,
                cancellationToken)
            ?? throw new InvalidOperationException(
                $"Saga state for order " +
                $"'{integrationEvent.OrderId}' was not found.");

        var utcNow = _timeProvider.GetUtcNow();

        order.MarkPaymentFailed(utcNow);

        sagaState.RecordPaymentFailure(
            integrationEvent.EventId);

        _inboxRepository.Add(
            integrationEvent.EventId,
            nameof(PaymentFailed),
            utcNow);
    }
}