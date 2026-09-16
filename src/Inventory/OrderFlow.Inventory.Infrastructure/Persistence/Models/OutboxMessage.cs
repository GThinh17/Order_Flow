namespace OrderFlow.Inventory.Infrastructure.Persistence.Models;

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    public OutboxMessage(
        Guid eventId,
        string eventType,
        string topic,
        string partitionKey,
        string payload,
        DateTimeOffset createdAt)
    {
        EventId = eventId;
        EventType = eventType;
        Topic = topic;
        PartitionKey = partitionKey;
        Payload = payload;
        CreatedAt = createdAt;
    }

    public long Id { get; private set; }

    public Guid EventId { get; private set; }

    public string EventType { get; private set; } = string.Empty;

    public string Topic { get; private set; } = string.Empty;

    public string PartitionKey { get; private set; } = string.Empty;

    public string Payload { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }

    public void MarkAsPublished(DateTimeOffset publishedAt)
    {
        if (PublishedAt is not null)
        {
            return;
        }

        PublishedAt = publishedAt;
    }
}
