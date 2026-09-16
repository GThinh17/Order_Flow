using System.Net.Sockets;
using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrderFlow.Inventory.Infrastructure.Persistence.Messaging;

public sealed class PulsarInventoryEventPublisher
    : IInventoryEventPublisher, IAsyncDisposable
{
    private readonly IPulsarClient _client;
    private readonly IProducer<string> _producer;

    public PulsarInventoryEventPublisher(
        IOptions<PulsarOptions> options,
        ILogger<PulsarInventoryEventPublisher> logger)
    {
        var pulsarOptions = options.Value;

        if (!Uri.TryCreate(
                pulsarOptions.ServiceURL,
                UriKind.Absolute,
                out var serviceUrl))
        {
            throw new InvalidOperationException(
                "Pulsar ServiceUrl is missing or invalid.");
        }

        _client = PulsarClient
            .Builder()
            .ServiceUrl(serviceUrl)
            .ExceptionHandler(context =>
            {
                if (context.Exception is not SocketException)
                {
                    return;
                }

                logger.LogWarning(
                    context.Exception,
                    "Pulsar connection failed; retrying.");

                context.Result = FaultAction.Retry;
                context.ExceptionHandled = true;
            })
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

        metadata["eventId"] = eventId.ToString("D");
        metadata["eventType"] = eventType;

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
