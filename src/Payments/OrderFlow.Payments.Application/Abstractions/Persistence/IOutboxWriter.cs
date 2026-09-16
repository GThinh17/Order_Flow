using OrderFlow.Contracts.IntegrationEvents.Payments;

namespace OrderFlow.Payments.Application.Abstractions.Persistence
{
    public interface IOutboxWriter
    {
        void Add(PaymentSucceeded paymentSucceeded);

        void Add(PaymentFailed paymentFailed);
    }
}
