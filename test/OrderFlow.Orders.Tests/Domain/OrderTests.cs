using OrderFlow.Orders.Domain;
using OrderFlow.Orders.Domain.Entity;
using OrderFlow.Orders.Domain.Enum;

namespace OrderFlow.Orders.Tests.Domain;

public sealed class OrderTests
{
    [Fact]
    public void Create_WithValidData_CreatesPendingOrderAndCalculatesTotal()
    {
        var createdAt = new DateTimeOffset(
            2026,
            9,
            12,
            10,
            15,
            0,
            TimeSpan.Zero);

        var lines = new[]
        {
            OrderLine.Create(
                "WIDGET-01",
                2,
                10.00m),

            OrderLine.Create(
                "WIDGET-02",
                1,
                5.50m)
        };

        var order = Order.Create(
            "cust-123",
            lines,
            createdAt);

        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.Equal("cust-123", order.CustomerId);
        Assert.Equal(Orders.Domain.Enum.OrderStatus.Pending, order.Status);
        Assert.Equal(25.50m, order.TotalAmount);
        Assert.Equal(2, order.Lines.Count);
        Assert.Equal(createdAt, order.CreatedAt);
        Assert.Equal(createdAt, order.UpdatedAt);
    }

    [Fact]
    public void Create_WithoutCustomerId_ThrowsArgumentException()
    {
        var lines = new[]
        {
            OrderLine.Create(
                "WIDGET-01",
                1,
                10.00m)
        };

        var action = () => Order.Create(
            " ",
            lines,
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Create_WithoutLines_ThrowsArgumentException()
    {
        var action = () => Order.Create(
            "cust-123",
            Array.Empty<OrderLine>(),
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void CreateLine_WithNonPositiveQuantity_ThrowsArgumentOutOfRangeException()
    {
        var action = () => OrderLine.Create(
            "WIDGET-01",
            0,
            10.00m);

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }

    [Fact]
    public void CreateLine_WithMoreThanTwoPriceDecimals_ThrowsArgumentException()
    {
        var action = () => OrderLine.Create(
            "WIDGET-01",
            1,
            10.001m);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void StartReserving_WhenPending_ChangesStatusToReserving()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var updatedAt = createdAt.AddSeconds(1);
        var order = CreateValidOrder(createdAt);

        order.StartReserving(updatedAt);

        Assert.Equal(OrderStatus.Reserving, order.Status);
        Assert.Equal(updatedAt, order.UpdatedAt);
    }

    [Fact]
    public void MarkReservationSucceeded_WhenReserving_ChangesStatusToCharging()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var updatedAt = createdAt.AddSeconds(2);
        var order = CreateValidOrder(createdAt);
        order.StartReserving(createdAt.AddSeconds(1));

        order.MarkReservationSucceeded(updatedAt);

        Assert.Equal(OrderStatus.Charging, order.Status);
        Assert.Equal(updatedAt, order.UpdatedAt);
    }

    [Fact]
    public void MarkReservationFailed_WhenReserving_ChangesStatusToCancelled()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var order = CreateValidOrder(createdAt);
        order.StartReserving(createdAt.AddSeconds(1));

        order.MarkReservationFailed(createdAt.AddSeconds(2));

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void MarkReservationSucceeded_WhenPending_ThrowsInvalidOperationException()
    {
        var order = CreateValidOrder(DateTimeOffset.UtcNow);

        var action = () => order.MarkReservationSucceeded(
            DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void MarkReservationFailed_WhenCharging_ThrowsInvalidOperationException()
    {
        var order = CreateValidOrder(DateTimeOffset.UtcNow);
        order.StartReserving(DateTimeOffset.UtcNow);
        order.MarkReservationSucceeded(DateTimeOffset.UtcNow);

        var action = () => order.MarkReservationFailed(
            DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(action);
    }

    private static Order CreateValidOrder(
        DateTimeOffset createdAt)
    {
        return Order.Create(
            "cust-123",
            [OrderLine.Create("WIDGET-01", 1, 10m)],
            createdAt);
    }
}
