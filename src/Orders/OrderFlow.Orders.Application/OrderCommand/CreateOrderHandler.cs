using OrderFlow.Contracts.IntegrationEvents.Orders;
using OrderFlow.Orders.Application.Abstractions.Messaging;
using OrderFlow.Orders.Application.Abstractions.Persistence;
using OrderFlow.Orders.Domain.Entity;

namespace OrderFlow.Orders.Application.OrderCommand
{
    public sealed class CreateOrderHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderSagaStateRepository _sagaStateRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOutboxWriter _outboxWriter;
        private readonly TimeProvider _timeProvider;

        public CreateOrderHandler(
            IOrderRepository orderRepository,
            IOrderSagaStateRepository sagaStateRepository,
            IUnitOfWork unitOfWork,
            IOutboxWriter outboxWriter,
            TimeProvider timeProvider
        )
        {
            _orderRepository = orderRepository;
            _sagaStateRepository = sagaStateRepository;
            _outboxWriter = outboxWriter;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async Task<CreateOrderResult> HandleAsync(
            CreateOrderCommand command,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(command.Lines);

            var orderLines = command.Lines
                .Select(line => OrderLine.Create(
                    line.Sku,
                    line.Quantity,
                    line.UnitPrice))
                .ToList();

            var utcNow = _timeProvider.GetUtcNow();

            var order = Order.Create(
                command.CustomerId,
                orderLines,
                utcNow);

            _orderRepository.Add(order);

            _sagaStateRepository.Add(
                OrderSagaState.Create(order.Id));

            var orderPlaced = new OrderPlaced(
                Guid.NewGuid(),
                order.Id,
                order.Id,
                utcNow,
                order.CustomerId,
                order.TotalAmount,
                order.Lines
                    .Select(line => new OrderPlacedLine(
                            line.Sku,
                            line.Quantity,
                            line.UnitPrice))
                    .ToArray());

            _outboxWriter.Add(orderPlaced);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateOrderResult(
                order.Id,
                order.Id,
                order.Status);
        }
    }
}
