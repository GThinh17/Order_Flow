namespace OrderFlow.Orders.Domain.Entity;

public sealed class OrderSagaState
{
    private OrderSagaState()
    {
    }

    private OrderSagaState(Guid orderId)
    {
        OrderId = orderId;
    }

    public Guid OrderId { get; private set; }

    public bool ReservationCompleted { get; private set; }

    public bool PaymentCompleted { get; private set; }

    public Guid? LastProcessedEventId { get; private set; }

    public static OrderSagaState Create(Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException(
                "Order ID is required.",
                nameof(orderId));
        }

        return new OrderSagaState(orderId);
    }

    public void CompleteReservation(Guid eventId)
    {
        if (eventId == Guid.Empty)
        {
            throw new ArgumentException(
                "Event ID is required.",
                nameof(eventId));
        }

        ReservationCompleted = true;
        LastProcessedEventId = eventId;
    }
    public void CompletePayment(Guid eventId)
    {
        if (eventId == Guid.Empty)
        {
            throw new ArgumentException(
                "Event ID is required.",
                nameof(eventId));
        }

        PaymentCompleted = true;
        LastProcessedEventId = eventId;
    }
    public void RecordPaymentFailure(Guid eventId)
    {
        if (eventId == Guid.Empty)
        {
            throw new ArgumentException(
                "Event ID is required.",
                nameof(eventId));
        }

        PaymentCompleted = false;
        LastProcessedEventId = eventId;
    }

}
