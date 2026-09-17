using OrderFlow.Payments.Infrastructure.PaymentGateWay;

namespace OrderFlow.Payments.Tests.Infrastructure;

public sealed class PaymentGatewayTests
{
    [Theory]
    [InlineData(25.50, true)]
    [InlineData(25.99, false)]
    [InlineData(100.99, false)]
    public async Task ChargeAsync_UsesNinetyNineCentsAsFailureRule(
        decimal amount,
        bool expectedSuccess)
    {
        // Arrange
        var gateway = new PaymentGateway();

        // Act
        var result = await gateway.ChargeAsync(
            Guid.NewGuid(),
            amount);

        // Assert
        Assert.Equal(expectedSuccess, result.IsSuccessful);
        Assert.Equal(
            expectedSuccess,
            result.FailureReason is null);
    }
}
