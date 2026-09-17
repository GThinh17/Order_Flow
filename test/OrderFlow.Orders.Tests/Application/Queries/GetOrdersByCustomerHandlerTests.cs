using OrderFlow.Orders.Application.Abstractions.Persistence;
using OrderFlow.Orders.Application.Handler;
using OrderFlow.Orders.Domain.Entity;

namespace OrderFlow.Orders.Tests.Application.Queries;

public sealed class GetOrdersByCustomerHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithMatchingCustomer_ReturnsSummaries()
    {
        // Arrange
        var createdAt = new DateTimeOffset(
            2026,
            9,
            17,
            12,
            0,
            0,
            TimeSpan.Zero);

        var matchingOrder = Order.Create(
            "customer-1",
            [OrderLine.Create("DEMO-SUCCESS", 2, 20.00m)],
            createdAt);

        var otherOrder = Order.Create(
            "customer-2",
            [OrderLine.Create("DEMO-FAILURE", 1, 19.99m)],
            createdAt);

        var handler = new GetOrdersByCustomerHandler(
            new FakeOrderRepository(
                matchingOrder,
                otherOrder));

        // Act
        var results = await handler.HandleAsync(" customer-1 ");

        // Assert
        var result = Assert.Single(results);

        Assert.Equal(matchingOrder.Id, result.OrderId);
        Assert.Equal("Pending", result.Status);
        Assert.Equal(40.00m, result.TotalAmount);
        Assert.Equal(createdAt, result.CreatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_WithoutCustomerId_ThrowsArgumentException(
        string customerId)
    {
        // Arrange
        var handler = new GetOrdersByCustomerHandler(
            new FakeOrderRepository());

        // Act
        var exception = await Record.ExceptionAsync(
            () => handler.HandleAsync(customerId));

        // Assert
        Assert.IsType<ArgumentException>(exception);
    }

    private sealed class FakeOrderRepository(params Order[] orders)
        : IOrderRepository
    {
        private readonly List<Order> _orders = [.. orders];

        public void Add(Order order)
        {
            _orders.Add(order);
        }

        public Task<Order?> GetByIdAsync(
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _orders.SingleOrDefault(order => order.Id == orderId));
        }

        public Task<Order?> GetDetailsByIdAsync(
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            return GetByIdAsync(orderId, cancellationToken);
        }

        public Task<IReadOnlyCollection<Order>> GetByCustomerIdAsync(
            string customerId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<Order> matches = _orders
                .Where(order => order.CustomerId == customerId)
                .ToArray();

            return Task.FromResult(matches);
        }
    }
}
