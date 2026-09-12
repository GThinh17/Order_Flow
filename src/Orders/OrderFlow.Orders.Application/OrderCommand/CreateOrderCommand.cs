namespace OrderFlow.Orders.Application.OrderCommand
{
    public sealed record CreateOrderCommand(
        string CustomerId,
        IReadOnlyCollection<CreateOrderLineCommand> Lines
    );
}