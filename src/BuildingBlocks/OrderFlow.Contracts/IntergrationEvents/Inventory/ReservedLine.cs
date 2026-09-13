namespace OrderFlow.Contracts.IntegrationEvents.Inventory
{
    public sealed record ReservedLine(
        string Sku,
        int Quantity);
}