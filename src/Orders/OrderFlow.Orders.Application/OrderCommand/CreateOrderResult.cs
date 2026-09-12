using OrderFlow.Orders.Domain.Enum;

namespace OrderFlow.Orders.Application.OrderCommand
{
    public sealed record CreateOrderResult(
        Guid OrderId,
        Guid CorrelationId,
        OrderStatus Status
    );
}