using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Application.Handler;
using OrderFlow.Payments.Domain.Entity;

namespace OrderFlow.Payments.Tests.Application;

public sealed class GetPaymentByOrderIdHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenPaymentExists_ReturnsResponse()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var payment = Payment.CreateSucceeded(
            orderId,
            25.50m,
            DateTimeOffset.UtcNow);
        var handler = new GetPaymentByOrderIdHandler(
            new FakePaymentRepository(payment));

        // Act
        var response = await handler.HandleAsync(orderId);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(payment.Id, response.PaymentId);
        Assert.Equal(orderId, response.OrderId);
        Assert.Equal(payment.Amount, response.Amount);
        Assert.Equal(payment.Status.ToString(), response.Status);
    }

    [Fact]
    public async Task HandleAsync_WhenPaymentDoesNotExist_ReturnsNull()
    {
        // Arrange
        var handler = new GetPaymentByOrderIdHandler(
            new FakePaymentRepository());

        // Act
        var response = await handler.HandleAsync(Guid.NewGuid());

        // Assert
        Assert.Null(response);
    }

    private sealed class FakePaymentRepository(
        params Payment[] payments) : IPaymentRepository
    {
        public Task<Payment?> GetByOrderIdAsync(
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                payments.SingleOrDefault(
                    payment => payment.OrderId == orderId));
        }

        public void Add(Payment payment)
        {
            throw new NotSupportedException();
        }
    }
}
