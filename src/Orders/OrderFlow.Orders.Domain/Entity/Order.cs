using OrderFlow.Orders.Domain.Constants;
using OrderFlow.Orders.Domain.Enum;
namespace OrderFlow.Orders.Domain.Entity
{
    public sealed class Order
    {
        private readonly List<OrderLine> _lines = [];

        private Order()
        {

        }

        public Guid Id { get; private set; }

        public string CustomerId { get; private set; } = string.Empty;

        public OrderStatus Status { get; private set; }

        public decimal TotalAmount { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset UpdatedAt { get; private set; }

        public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

        public static Order Create(
            string customerId,
            IEnumerable<OrderLine> line,
            DateTimeOffset utcNow)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                throw new ArgumentException(
                    DomainMessageError.RequiredCustomerIdError,
                    nameof(customerId)
                );
            }

            var validCutomerId = customerId.Trim();

            if (validCutomerId.Length > 100)
            {
                throw new ArgumentException(
                    DomainMessageError.InvalidCustomerIdError,
                    nameof(customerId));
            }

            ArgumentNullException.ThrowIfNull(line);

            var orderLines = line.ToList();

            if (orderLines.Count is 0)
            {
                throw new ArgumentException(
                    DomainMessageError.AcceptableOrderLineError,
                    nameof(line)
                );
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = validCutomerId,
                TotalAmount = orderLines.Sum(
                    line => line.TotalAmount),
                Status = OrderStatus.Pending,
                CreatedAt = utcNow,
                UpdatedAt = utcNow
            };

            order._lines.AddRange(orderLines);

            return order;
        }

        public void StartReserving(
            DateTimeOffset utcNow)
        {
            if (Status == OrderStatus.Reserving)
            {
                return;
            }

            if (Status != OrderStatus.Pending)
            {
                throw new InvalidOperationException(
                    $"Order cannot transition from {Status} to Reserving.");
            }

            Status = OrderStatus.Reserving;
            UpdatedAt = utcNow;
        }

        public void MarkReservationSucceeded(
            DateTimeOffset utcNow)
        {
            if (Status == OrderStatus.Charging)
            {
                return;
            }

            if (Status != OrderStatus.Reserving)
            {
                throw new InvalidOperationException(
                    $"Order cannot transition from {Status} to Charging.");
            }

            Status = OrderStatus.Charging;
            UpdatedAt = utcNow;
        }

        public void MarkReservationFailed(
            DateTimeOffset utcNow)
        {
            if (Status == OrderStatus.Cancelled)
            {
                return;
            }

            if (Status != OrderStatus.Reserving)
            {
                throw new InvalidOperationException(
                    $"Order cannot transition from {Status} to Cancelled after reservation failure.");
            }

            Status = OrderStatus.Cancelled;
            UpdatedAt = utcNow;
        }
    }
}
