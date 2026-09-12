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

        public Guid Id { get; set; }

        public string CustomerId { get; private set; } = string.Empty;

        public OrderStatus Status { get; private set; }

        public decimal TotalAmount { get; private set; }

        public DateTimeOffset CreateAt { get; private set; }

        public DateTimeOffset UpdateAt { get; private set; }

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
                CreateAt = utcNow,
                UpdateAt = utcNow
            };

            order._lines.AddRange(orderLines);

            return order;
        }
    }
}