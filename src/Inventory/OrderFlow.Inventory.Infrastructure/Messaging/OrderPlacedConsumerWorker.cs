using System.Globalization;
using System.Text.Json;
using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderFlow.Contracts.IntegrationEvents.Orders;
using OrderFlow.Contracts.IntegrationEvents.Payments;
using OrderFlow.Inventory.Application.Handler;

namespace OrderFlow.Inventory.Infrastructure.Persistence.Messaging;

public sealed class OrderPlacedConsumerWorker : BackgroundService
{
    private const string EventTypeProperty = "eventType";

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private static readonly TimeSpan ReconnectDelay =
        TimeSpan.FromSeconds(2);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly PulsarOptions _options;
    private readonly ILogger<OrderPlacedConsumerWorker> _logger;

    public OrderPlacedConsumerWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<PulsarOptions> options,
        ILogger<OrderPlacedConsumerWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var serviceUrl = new Uri(_options.ServiceURL);

        await using var client = PulsarClient
            .Builder()
            .ServiceUrl(serviceUrl)
            .Build();

        await using var consumer = client
            .NewConsumer(Schema.String)
            .Topic(_options.Topic)
            .SubscriptionName(_options.SubscriptionName)
            .SubscriptionType(SubscriptionType.KeyShared)
            .InitialPosition(SubscriptionInitialPosition.Earliest)
            .MessagePrefetchCount(20)
            .Create();

        await using var deadLetterProducer = client
            .NewProducer(Schema.String)
            .Topic(_options.DeadLetterTopic)
            .Create();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeMessagesAsync(
                    consumer,
                    deadLetterProducer,
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Inventory Pulsar consumer stopped unexpectedly.");

                await Task.Delay(
                    ReconnectDelay,
                    stoppingToken);
            }
        }
    }

    private async Task ConsumeMessagesAsync(
        IConsumer<string> consumer,
        IProducer<string> deadLetterProducer,
        CancellationToken cancellationToken)
    {
        await foreach (
            var message in consumer.Messages(cancellationToken))
        {
            try
            {
                await DispatchAsync(
                    message,
                    cancellationToken);

                await consumer.Acknowledge(
                    message,
                    cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                await HandleFailedMessageAsync(
                    consumer,
                    deadLetterProducer,
                    message,
                    exception,
                    cancellationToken);
            }
        }
    }

    private async Task DispatchAsync(
        IMessage<string> message,
        CancellationToken cancellationToken)
    {
        if (!message.Properties.TryGetValue(
                EventTypeProperty,
                out var eventType))
        {
            throw new InvalidDataException(
                "Pulsar message does not contain eventType.");
        }

        await using var scope =
            _scopeFactory.CreateAsyncScope();

        switch (eventType)
        {
            case nameof(OrderPlaced):
                {
                    var integrationEvent =
                        Deserialize<OrderPlaced>(
                            message.Value());

                    var handler = scope.ServiceProvider
                        .GetRequiredService<ReserveOrderHandler>();

                    await handler.HandleAsync(
                        integrationEvent,
                        cancellationToken);

                    break;
                }

            case nameof(PaymentSucceeded):
                {
                    var integrationEvent =
                        Deserialize<PaymentSucceeded>(
                            message.Value());

                    var handler = scope.ServiceProvider
                        .GetRequiredService<ConsumeReservationHandler>();

                    await handler.HandleAsync(
                        integrationEvent,
                        cancellationToken);

                    break;
                }

            case nameof(PaymentFailed):
                {
                    var integrationEvent =
                        Deserialize<PaymentFailed>(
                            message.Value());

                    var handler = scope.ServiceProvider
                        .GetRequiredService<ReleaseReservationHandler>();

                    await handler.HandleAsync(
                        integrationEvent,
                        cancellationToken);

                    break;
                }

            default:
                _logger.LogDebug(
                    "Inventory ignored event type {EventType}.",
                    eventType);

                break;
        }
    }

    private static TEvent Deserialize<TEvent>(
        string payload)
    {
        return JsonSerializer.Deserialize<TEvent>(
                payload,
                JsonOptions)
            ?? throw new JsonException(
                $"{typeof(TEvent).Name} payload is null.");
    }

    private async Task HandleFailedMessageAsync(
        IConsumer<string> consumer,
        IProducer<string> deadLetterProducer,
        IMessage<string> message,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var deliveryAttempt =
            checked((int)message.RedeliveryCount + 1);

        if (deliveryAttempt < _options.MaxDeliveryAttempts)
        {
            _logger.LogWarning(
                exception,
                "Failed Inventory message attempt {Attempt}/{Maximum}.",
                deliveryAttempt,
                _options.MaxDeliveryAttempts);

            await Task.Delay(
                TimeSpan.FromSeconds(
                    _options.RedeliveryDelaySeconds),
                cancellationToken);

            await consumer.RedeliverUnacknowledgedMessages(
                [message.MessageId],
                cancellationToken);

            return;
        }

        await PublishToDeadLetterAsync(
            deadLetterProducer,
            message,
            deliveryAttempt,
            exception,
            cancellationToken);

        // Chỉ ACK message gốc sau khi publish DLQ thành công.
        await consumer.Acknowledge(
            message,
            cancellationToken);

        _logger.LogError(
            exception,
            "Inventory message moved to DLQ after {Attempt} attempts.",
            deliveryAttempt);
    }

    private async Task PublishToDeadLetterAsync(
        IProducer<string> deadLetterProducer,
        IMessage<string> message,
        int deliveryAttempt,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var metadata = new MessageMetadata
        {
            Key = message.Key
        };

        // Giữ lại eventId, eventType và metadata ban đầu.
        foreach (var property in message.Properties)
        {
            metadata[property.Key] = property.Value;
        }

        metadata["sourceTopic"] = _options.Topic;

        metadata["deliveryAttempts"] =
            deliveryAttempt.ToString(
                CultureInfo.InvariantCulture);

        metadata["failureType"] =
            exception.GetType().Name;

        var failureMessage = exception.Message;

        metadata["failureMessage"] =
            failureMessage[
                ..Math.Min(failureMessage.Length, 500)];

        await deadLetterProducer.Send(
            metadata,
            message.Value(),
            cancellationToken);
    }
}
