using OrderFlow.Orders.Application.Abstractions.Persistence;
using OrderFlow.Orders.Domain.Entity;

namespace OrderFlow.Orders.Application.OrderCommand
{
    public sealed class CreateOrderHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public CreateOrderHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider
        )
        {
            _orderRepository = orderRepository;
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

            var order = Order.Create(
                command.CustomerId,
                orderLines,
                _timeProvider.GetUtcNow());

            _orderRepository.Add(order);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateOrderResult(
                order.Id,
                order.Id,
                order.Status);
        }
    }
}