using OrderFlow.Contracts.IntegrationEvents.Inventory;
using OrderFlow.Contracts.IntegrationEvents.Payments;
using OrderFlow.Payments.Application.Abstractions.PaymentGateWay;
using OrderFlow.Payments.Application.Abstractions.Payments;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Application.Handler;
using OrderFlow.Payments.Domain.Entity;
using OrderFlow.Payments.Domain.Enum;

namespace OrderFlow.Payments.Tests.Application;

public sealed class ProcessReservationSucceededHandlerTests
{
    private static readonly DateTimeOffset UtcNow =
        new(2026, 9, 16, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task HandleAsync_WhenGatewaySucceeds_PersistsPaymentAndSuccessEvent()
    {
        // Arrange
        var gateway = new FakePaymentGateway(
            PaymentGateWayResult.Success());
        var paymentRepository = new FakePaymentRepository();
        var inboxRepository = new FakeInboxRepository();
        var outboxWriter = new FakeOutboxWriter();
        var handler = CreateHandler(
            gateway,
            paymentRepository,
            inboxRepository,
            outboxWriter);
        var integrationEvent = CreateEvent(25.50m);

        // Act
        await handler.HandleAsync(integrationEvent);

        // Assert
        var payment = Assert.Single(paymentRepository.Payments);
        Assert.Equal(integrationEvent.OrderId, payment.OrderId);
        Assert.Equal(integrationEvent.Total, payment.Amount);
        Assert.Equal(Status.Succeeded, payment.Status);
        Assert.Null(payment.FailureReason);

        var publishedEvent = Assert.Single(outboxWriter.Succeeded);
        Assert.Equal(payment.Id, publishedEvent.PaymentId);
        Assert.Equal(integrationEvent.OrderId, publishedEvent.OrderId);
        Assert.Contains(integrationEvent.EventId, inboxRepository.EventIds);
        Assert.Equal(1, gateway.CallCount);
    }

    [Fact]
    public async Task HandleAsync_WhenGatewayFails_PersistsPaymentAndFailureEvent()
    {
        // Arrange
        const string failureReason = "Payment was declined.";
        var gateway = new FakePaymentGateway(
            PaymentGateWayResult.Failure(failureReason));
        var paymentRepository = new FakePaymentRepository();
        var inboxRepository = new FakeInboxRepository();
        var outboxWriter = new FakeOutboxWriter();
        var handler = CreateHandler(
            gateway,
            paymentRepository,
            inboxRepository,
            outboxWriter);
        var integrationEvent = CreateEvent(25.99m);

        // Act
        await handler.HandleAsync(integrationEvent);

        // Assert
        var payment = Assert.Single(paymentRepository.Payments);
        Assert.Equal(Status.Failed, payment.Status);
        Assert.Equal(failureReason, payment.FailureReason);

        var publishedEvent = Assert.Single(outboxWriter.Failed);
        Assert.Equal(payment.Id, publishedEvent.PaymentId);
        Assert.Equal(failureReason, publishedEvent.Reason);
        Assert.Empty(outboxWriter.Succeeded);
        Assert.Contains(integrationEvent.EventId, inboxRepository.EventIds);
    }

    [Fact]
    public async Task HandleAsync_WhenEventWasProcessed_DoesNotChargeAgain()
    {
        // Arrange
        var integrationEvent = CreateEvent(25.50m);
        var gateway = new FakePaymentGateway(
            PaymentGateWayResult.Success());
        var paymentRepository = new FakePaymentRepository();
        var inboxRepository = new FakeInboxRepository(
            integrationEvent.EventId);
        var outboxWriter = new FakeOutboxWriter();
        var handler = CreateHandler(
            gateway,
            paymentRepository,
            inboxRepository,
            outboxWriter);

        // Act
        await handler.HandleAsync(integrationEvent);

        // Assert
        Assert.Equal(0, gateway.CallCount);
        Assert.Empty(paymentRepository.Payments);
        Assert.Empty(outboxWriter.Succeeded);
        Assert.Empty(outboxWriter.Failed);
    }

    private static ProcessReservationSucceededHandler CreateHandler(
        IPaymentGateWay gateway,
        IPaymentRepository paymentRepository,
        IInboxRepository inboxRepository,
        IOutboxWriter outboxWriter)
    {
        return new ProcessReservationSucceededHandler(
            gateway,
            paymentRepository,
            inboxRepository,
            outboxWriter,
            new ImmediateTransactionRunner(),
            new FixedTimeProvider(UtcNow));
    }

    private static ReservationSucceeded CreateEvent(decimal total)
    {
        return new ReservationSucceeded(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            UtcNow,
            Guid.NewGuid(),
            total,
            [new ReservedLine("WIDGET-01", 2)]);
    }

    private sealed class FakePaymentGateway(
        PaymentGateWayResult result) : IPaymentGateWay
    {
        public int CallCount { get; private set; }

        public Task<PaymentGateWayResult> ChargeAsync(
            Guid orderId,
            decimal amount,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(result);
        }
    }

    private sealed class FakePaymentRepository : IPaymentRepository
    {
        public List<Payment> Payments { get; } = [];

        public Task<Payment?> GetByOrderIdAsync(
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Payments.SingleOrDefault(
                    payment => payment.OrderId == orderId));
        }

        public void Add(Payment payment)
        {
            Payments.Add(payment);
        }
    }

    private sealed class FakeInboxRepository : IInboxRepository
    {
        public FakeInboxRepository(params Guid[] eventIds)
        {
            EventIds = eventIds.ToHashSet();
        }

        public HashSet<Guid> EventIds { get; }

        public Task<bool> ExistAsync(
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
        public List<PaymentSucceeded> Succeeded { get; } = [];
        public List<PaymentFailed> Failed { get; } = [];

        public void Add(PaymentSucceeded paymentSucceeded)
        {
            Succeeded.Add(paymentSucceeded);
        }

        public void Add(PaymentFailed paymentFailed)
        {
            Failed.Add(paymentFailed);
        }
    }

    private sealed class ImmediateTransactionRunner
        : IPaymentTransactionRunner
    {
        public Task ExecuteAsync(
            Func<CancellationToken, Task> operation,
            CancellationToken cancellationToken = default)
        {
            return operation(cancellationToken);
        }
    }

    private sealed class FixedTimeProvider(
        DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
