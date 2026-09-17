using OrderFlow.Orders.Application.Abstractions.Persistence;
using OrderFlow.Orders.Application.OrderQuery;

namespace OrderFlow.Orders.Application.Handler;

public sealed class GetOrdersByCustomerHandler
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersByCustomerHandler(
        IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IReadOnlyCollection<GetOrderSummaryResult>>
        HandleAsync(
            string customerId,
            CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            throw new ArgumentException(
                "Customer ID is required.",
                nameof(customerId));
        }

        var normalizedCustomerId = customerId.Trim();

        if (normalizedCustomerId.Length > 100)
        {
            throw new ArgumentException(
                "Customer ID cannot exceed 100 characters.",
                nameof(customerId));
        }

        var orders = await _orderRepository.GetByCustomerIdAsync(
            normalizedCustomerId,
            cancellationToken);

        return orders
            .Select(order => new GetOrderSummaryResult(
                order.Id,
                order.Status.ToString(),
                order.TotalAmount,
                order.CreatedAt))
            .ToArray();
    }
}
