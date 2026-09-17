using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderFlow.Messaging;

namespace OrderFlow.Orders.Infrastructure.Messaging
{
    public sealed class OrdersConsumer : BackgroundService
    {
        private const string EventTypeProperty = "eventType";

        private static readonly TimeSpan ReconnectDelay =
            TimeSpan.FromSeconds(2);

        private readonly IOrdersEventDispatcher _dispatcher;
        private readonly PulsarOptions _options;
        private readonly ILogger<OrdersConsumer> _logger;
        private readonly PulsarFailedMessageHandler _failedMessageHandler;

        public OrdersConsumer(
            IOrdersEventDispatcher dispatcher,
            IOptions<PulsarOptions> options,
            ILogger<OrdersConsumer> logger,
            PulsarFailedMessageHandler failedMessageHandler)
        {
            _dispatcher = dispatcher;
            _options = options.Value;
            _logger = logger;
            _failedMessageHandler = failedMessageHandler;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            var serviceUrl = new Uri(_options.ServiceUrl);

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
                        "Orders Pulsar consumer stopped unexpectedly.");

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
                    if (!message.Properties.TryGetValue(
                            EventTypeProperty,
                            out var eventType))
                    {
                        throw new InvalidDataException(
                            "Pulsar message does not contain eventType.");
                    }

                    await _dispatcher.DispatchAsync(
                        eventType,
                        message.Value(),
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
                    await _failedMessageHandler.HandleAsync(
                        consumer,
                        deadLetterProducer,
                        message,
                        exception,
                        cancellationToken);
                }
            }
        }
    }
}
