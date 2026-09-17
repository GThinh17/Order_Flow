namespace OrderFlow.Blazor.Options;

public sealed record ApiUrlsOptions(
    Uri Orders,
    Uri Inventory);