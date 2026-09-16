using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Application.Handler;
using OrderFlow.Payments.Domain.Entity;

namespace OrderFlow.Payments.Tests.Application;

public sealed class GetPaymentByOrderIdHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenPaymentExists_ReturnsResponse()
    {
        var orderId = Guid.NewGuid();
        var payment = Payment.CreateSucceeded(
            orderId,
            25.50m,
            DateTimeOffset.UtcNow);
        var handler = new GetPaymentByOrderIdHandler(
            new FakePaymentRepository(payment));

        var response = await handler.HandleAsync(orderId);

        Assert.NotNull(response);
        Assert.Equal(payment.Id, response.PaymentId);
        Assert.Equal(orderId, response.OrderId);
        Assert.Equal(payment.Amount, response.Amount);
        Assert.Equal(payment.Status.ToString(), response.Status);
    }

    [Fact]
    public async Task HandleAsync_WhenPaymentDoesNotExist_ReturnsNull()
    {
        var handler = new GetPaymentByOrderIdHandler(
            new FakePaymentRepository());

        var response = await handler.HandleAsync(Guid.NewGuid());

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
