
namespace OrderFlow.Orders.Infrastructure.Persistence.Repositories
{
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
            DateTimeOffset createAt)
        {
            EventId = eventId;
            EventType = eventType;
            Topic = topic;
            PartitionKey = partitionKey;
            Payload = payload;
            CreateAt = createAt;
        }

        public long Id { get; private set; }
        public Guid EventId { get; private set; }
        public string EventType { get; private set; } = string.Empty;
        public string Topic { get; private set; } = string.Empty;
        public string PartitionKey { get; private set; } = string.Empty;
        public string Payload { get; private set; } = string.Empty;
        public DateTimeOffset CreateAt { get; private set; }
        public DateTimeOffset? PublishAt { get; private set; }
    }
}