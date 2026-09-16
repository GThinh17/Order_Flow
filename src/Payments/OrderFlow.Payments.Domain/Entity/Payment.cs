using OrderFlow.Payments.Domain.Enum;

namespace OrderFlow.Payments.Domain.Entity
{
    public sealed class Payment
    {
        private const int MaxFailureReasonLength = 500;

        private Payment()
        {
        }

        private Payment(
            Guid id,
            Guid orderId,
            decimal amount,
            Status status,
            string? failureReason,
            DateTimeOffset createdAt)
        {
            Id = id;
            OrderId = orderId;
            Amount = amount;
            Status = status;
            FailureReason = failureReason;
            CreatedAt = createdAt;
        }
        public Guid Id { get; private set; }
        public Guid OrderId { get; private set; }
        public decimal Amount { get; private set; }
        public Status Status { get; private set; }
        public string? FailureReason { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        public static Payment CreateSucceeded(
            Guid orderId,
            decimal amount,
            DateTimeOffset utcNow)
        {
            return Create(
                orderId,
                amount,
                Status.Succeeded,
                failureReason: null,
                utcNow);
        }

        public static Payment CreateFailed(
            Guid orderId,
            decimal amount,
            string failureReason,
            DateTimeOffset utcNow)
        {
            if (string.IsNullOrWhiteSpace(failureReason))
            {
                throw new ArgumentException(
                    "Failure reason is required.",
                    nameof(failureReason));
            }

            var normalizedReason = failureReason.Trim();

            if (normalizedReason.Length > MaxFailureReasonLength)
            {
                throw new ArgumentException(
                    $"Failure reason cannot exceed {MaxFailureReasonLength} characters.",
                    nameof(failureReason));
            }

            return Create(
                orderId,
                amount,
                Status.Failed,
                normalizedReason,
                utcNow);
        }

        private static Payment Create(
            Guid orderId,
            decimal amount,
            Status status,
            string? failureReason,
            DateTimeOffset utcNow)
        {
            if (orderId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Order ID is required.",
                    nameof(orderId));
            }

            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    amount,
                    "Payment amount must be greater than zero.");
            }

            return new Payment(
                Guid.NewGuid(),
                orderId,
                amount,
                status,
                failureReason,
                utcNow);
        }
    }
}
