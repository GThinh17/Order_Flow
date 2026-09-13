namespace OrderFlow.Orders.Infrastructure.Messaging
{
    public sealed class PulsarOptions
    {
        public const string SectionName = "Pulsar";

        public string ServiceUrl { get; set; } = string.Empty;

        public string Topic { get; set; } = string.Empty;
    }
}