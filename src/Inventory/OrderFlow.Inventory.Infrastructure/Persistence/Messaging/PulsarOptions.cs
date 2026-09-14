namespace OrderFlow.Inventory.Infrastructure.Persistence.Messaging
{
    public sealed class PulsarOptions
    {
        public const string SectionName = "Pulsar";

        public string ServiceURL { get; set; } = string.Empty;

        public string Topic { get; set; } = string.Empty;

        public string SubscriptionName { get; set; } = string.Empty;

        public string DeadLetterTopic { get; set; } = string.Empty;

        public int MaxDeliveryAttempts { get; set; } = 3;

        public int RedeliveryDelaySeconds { get; set; } = 2;
    }
}