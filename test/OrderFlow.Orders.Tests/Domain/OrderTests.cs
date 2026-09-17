using OrderFlow.Orders.Domain;
using OrderFlow.Orders.Domain.Entity;
using OrderFlow.Orders.Domain.Enum;

namespace OrderFlow.Orders.Tests.Domain;

public sealed class OrderTests
{
    [Fact]
    public void Create_WithValidData_CreatesPendingOrderAndCalculatesTotal()
    {
        // Arrange
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

        // Act
        var order = Order.Create(
            "cust-123",
            lines,
            createdAt);

        // Assert
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
        // Arrange
        var lines = new[]
        {
            OrderLine.Create(
                "WIDGET-01",
                1,
                10.00m)
        };

        // Act
        var action = () => Order.Create(
            " ",
            lines,
            DateTimeOffset.UtcNow);

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Create_WithoutLines_ThrowsArgumentException()
    {
        // Arrange
        var lines = Array.Empty<OrderLine>();

        // Act
        var action = () => Order.Create(
            "cust-123",
            lines,
            DateTimeOffset.UtcNow);

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void CreateLine_WithNonPositiveQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        const int quantity = 0;

        // Act
        var action = () => OrderLine.Create(
            "WIDGET-01",
            quantity,
            10.00m);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(action);
    }

    [Fact]
    public void CreateLine_WithMoreThanTwoPriceDecimals_ThrowsArgumentException()
    {
        // Arrange
        const decimal unitPrice = 10.001m;

        // Act
        var action = () => OrderLine.Create(
            "WIDGET-01",
            1,
            unitPrice);

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void StartReserving_WhenPending_ChangesStatusToReserving()
    {
        // Arrange
        var createdAt = DateTimeOffset.UtcNow;
        var updatedAt = createdAt.AddSeconds(1);
        var order = CreateValidOrder(createdAt);

        // Act
        order.StartReserving(updatedAt);

        // Assert
        Assert.Equal(OrderStatus.Reserving, order.Status);
        Assert.Equal(updatedAt, order.UpdatedAt);
    }

    [Fact]
    public void MarkReservationSucceeded_WhenReserving_ChangesStatusToCharging()
    {
        // Arrange
        var createdAt = DateTimeOffset.UtcNow;
        var updatedAt = createdAt.AddSeconds(2);
        var order = CreateValidOrder(createdAt);
        order.StartReserving(createdAt.AddSeconds(1));

        // Act
        order.MarkReservationSucceeded(updatedAt);

        // Assert
        Assert.Equal(OrderStatus.Charging, order.Status);
        Assert.Equal(updatedAt, order.UpdatedAt);
    }

    [Fact]
    public void MarkReservationFailed_WhenReserving_ChangesStatusToCancelled()
    {
        // Arrange
        var createdAt = DateTimeOffset.UtcNow;
        var order = CreateValidOrder(createdAt);
        order.StartReserving(createdAt.AddSeconds(1));

        // Act
        order.MarkReservationFailed(createdAt.AddSeconds(2));

        // Assert
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void MarkReservationSucceeded_WhenPending_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = CreateValidOrder(DateTimeOffset.UtcNow);

        // Act
        var action = () => order.MarkReservationSucceeded(
            DateTimeOffset.UtcNow);

        // Assert
        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void MarkReservationFailed_WhenCharging_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = CreateValidOrder(DateTimeOffset.UtcNow);
        order.StartReserving(DateTimeOffset.UtcNow);
        order.MarkReservationSucceeded(DateTimeOffset.UtcNow);

        // Act
        var action = () => order.MarkReservationFailed(
            DateTimeOffset.UtcNow);

        // Assert
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
