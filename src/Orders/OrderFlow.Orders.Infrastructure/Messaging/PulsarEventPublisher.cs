using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using Microsoft.Extensions.Options;

namespace OrderFlow.Orders.Infrastructure.Messaging
{
    public sealed class PulsarEventPublisher
        : IEventPublisher, IAsyncDisposable
    {
        private readonly IPulsarClient _client;
        private readonly IProducer<string> _producer;

        public PulsarEventPublisher(
            IOptions<PulsarOptions> options)
        {
            var pulsarOptions = options.Value;

            if (!Uri.TryCreate(
                pulsarOptions.ServiceUrl,
                UriKind.Absolute,
                out var serviceUrl))
            {
                throw new InvalidOperationException(
                    "Pulsar Topic is missing");
            }

            _client = PulsarClient
                .Builder()
                .ServiceUrl(serviceUrl)
                .Build();

            _producer = _client
                .NewProducer(Schema.String)
                .Topic(pulsarOptions.Topic)
                .Create();
        }

        public async ValueTask PublishAsync(
            Guid eventId,
            string eventType,
            string partitionKey,
            string payload,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(eventType);

            ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);

            ArgumentException.ThrowIfNullOrWhiteSpace(payload);

            var metadata = new MessageMetadata
            {
                Key = partitionKey
            };

            metadata["eventId"] =
                eventId.ToString("D");

            metadata["eventType"] =
                eventType;

            await _producer.Send(
                metadata,
                payload,
                cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            await _producer.DisposeAsync();
            await _client.DisposeAsync();
        }
    }
}