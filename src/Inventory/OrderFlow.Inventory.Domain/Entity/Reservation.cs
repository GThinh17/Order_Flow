using OrderFlow.Inventory.Domain.Enum;

namespace OrderFlow.Inventory.Domain.Entity
{
    public sealed class Reservation
    {
        private Reservation()
        {
        }

        private Reservation(
            Guid id,
            Guid orderId,
            Guid reservationId,
            string sku,
            int quantity,
            DateTimeOffset createdAt)
        {
            Id = id;
            ReservationId = reservationId;
            OrderId = orderId;
            Sku = sku;
            Quantity = quantity;
            Status = ReservationStatus.Active;
            CreatedAt = createdAt;
            UpdatedAt = createdAt;
        }

        public Guid Id { get; private set; }
        public Guid ReservationId { get; private set; }
        public Guid OrderId { get; private set; }
        public string Sku { get; private set; } = string.Empty;
        public int Quantity { get; private set; }
        public ReservationStatus Status { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        public static Reservation CreateActive(
            Guid reservationId,
            Guid orderId,
            string sku,
            int quantity,
            DateTimeOffset utcNow)
        {
            if (reservationId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Reservation ID is required.",
                    nameof(reservationId));
            }

            if (orderId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Order ID is required.",
                    nameof(orderId));
            }

            if (string.IsNullOrWhiteSpace(sku))
            {
                throw new ArgumentException(
                    "SKU is required.",
                    nameof(sku));
            }

            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(quantity));
            }

            return new Reservation(
                Guid.NewGuid(),
                reservationId,
                orderId,
                sku.Trim(),
                quantity,
                utcNow);
        }

        public void Release(DateTimeOffset utcNow)
        {
            if (Status == ReservationStatus.Released)
            {
                return;
            }

            if (Status != ReservationStatus.Active)
            {
                throw new InvalidOperationException(
                    "Only an active reservation can be released"
                );
            }

            Status = ReservationStatus.Released;
            UpdatedAt = utcNow;
        }

        public void Consume(DateTimeOffset utcNow)
        {
            if (Status == ReservationStatus.Consumed)
            {
                return;
            }

            if (Status != ReservationStatus.Active)
            {
                throw new InvalidOperationException(
                    "Only an active reservation can be consumed"
                );
            }

            Status = ReservationStatus.Consumed;
            UpdatedAt = utcNow;
        }
    }
}