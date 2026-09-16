namespace OrderFlow.Payments.Application.Contracts
{
    public sealed record PaymentResponse(
        Guid PaymentId,
        Guid OrderId,
        decimal Amount,
        string Status,
        DateTimeOffset CreatedAt);
}
