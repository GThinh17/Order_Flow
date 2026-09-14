namespace OrderFlow.Inventory.Infrastructure.Persistence.Messaging
{
    public sealed class InboxMessage
    {
        private InboxMessage()
        {
        }

        public InboxMessage(
            Guid eventId,
            string eventType,
            DateTimeOffset processedAt)
        {
            EventId = eventId;
            EventType = eventType;
            ProcessedAt = processedAt;
        }

        public Guid EventId { get; private set; }
        public string EventType { get; private set; } = string.Empty;
        public DateTimeOffset ProcessedAt { get; private set; }
    }
}
