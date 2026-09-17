using OrderFlow.Orders.Application.Abstractions.Persistence;
using OrderFlow.Orders.Application.OrderQuery;

namespace OrderFlow.Orders.Application.Handler
{
    public sealed class GetOrderByIdHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderSagaStateRepository _sagaStateRepository;

        public GetOrderByIdHandler(
            IOrderRepository orderRepository,
            IOrderSagaStateRepository sagaStateRepository)
        {
            _orderRepository = orderRepository;
            _sagaStateRepository = sagaStateRepository;
        }

        public async Task<GetOrderByIdResult?> HandleAsync(
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            var order = await _orderRepository.GetDetailsByIdAsync(
                orderId,
                cancellationToken);

            if (order is null)
            {
                return null;
            }

            var sagaState =
                await _sagaStateRepository.GetByOrderIdAsync(
                    orderId,
                    cancellationToken)
                ?? throw new InvalidOperationException(
                    $"Saga state for order '{orderId}' was not found.");

            var lines = order.Lines
                .Select(line => new GetOrderLineResult(
                    line.Sku,
                    line.Quantity,
                    line.UnitPrice))
                .ToArray();

            return new GetOrderByIdResult(
                order.Id,
                order.CustomerId,
                order.Status.ToString(),
                order.TotalAmount,
                sagaState.ReservationCompleted,
                sagaState.PaymentCompleted,
                lines,
                order.CreatedAt,
                order.UpdatedAt);
        }
    }
}

