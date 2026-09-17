using System.Globalization;
using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrderFlow.Messaging
{
    public sealed class PulsarFailedMessageHandler
    {
        private const int MaximumFailureMessageLength = 500;

        private readonly PulsarOptions _options;
        private readonly ILogger<PulsarFailedMessageHandler> _logger;

        public PulsarFailedMessageHandler(
            IOptions<PulsarOptions> options,
            ILogger<PulsarFailedMessageHandler> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task HandleAsync(
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
                    "Subscription {Subscription} failed message attempt " +
                    "{Attempt}/{Maximum}.",
                    _options.SubscriptionName,
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

            await consumer.Acknowledge(
                message,
                cancellationToken);

            _logger.LogError(
                exception,
                "Subscription {Subscription} moved message to DLQ " +
                "after {Attempt} attempts.",
                _options.SubscriptionName,
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

            foreach (var property in message.Properties)
            {
                metadata[property.Key] = property.Value;
            }

            metadata["sourceTopic"] = _options.Topic;
            metadata["sourceSubscription"] =
                _options.SubscriptionName;

            metadata["deliveryAttempts"] =
                deliveryAttempt.ToString(
                    CultureInfo.InvariantCulture);

            metadata["failureType"] =
                exception.GetType().Name;

            var failureMessage = exception.Message;

            metadata["failureMessage"] =
                failureMessage[
                    ..Math.Min(
                        failureMessage.Length,
                        MaximumFailureMessageLength)];

            await deadLetterProducer.Send(
                metadata,
                message.Value(),
                cancellationToken);
        }
    }
}
