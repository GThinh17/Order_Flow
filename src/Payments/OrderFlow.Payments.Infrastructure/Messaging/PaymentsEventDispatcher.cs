using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderFlow.Contracts.IntegrationEvents.Inventory;
using OrderFlow.Payments.Application.Handler;

namespace OrderFlow.Payments.Infrastructure.Messaging
{
    public sealed class PaymentsEventDispatcher : IPaymentsEventDispatcher
    {
        private static readonly JsonSerializerOptions JsonOptions =
            new(JsonSerializerDefaults.Web);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PaymentsEventDispatcher> _logger;

        public PaymentsEventDispatcher(
            IServiceScopeFactory scopeFactory,
            ILogger<PaymentsEventDispatcher> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task DispatchAsync(
            string eventType,
            string payload,
            CancellationToken cancellationToken)
        {
            if (!string.Equals(
                    eventType,
                    nameof(ReservationSucceeded),
                    StringComparison.Ordinal))
            {
                _logger.LogDebug(
                    "Payments ignored event type {EventType}.",
                    eventType);

                return;
            }

            var integrationEvent =
                JsonSerializer.Deserialize<ReservationSucceeded>(
                    payload,
                    JsonOptions)
                ?? throw new JsonException(
                    "ReservationSucceeded payload is null.");

            await using var scope =
                _scopeFactory.CreateAsyncScope();

            var handler = scope.ServiceProvider
                .GetRequiredService<
                    ProcessReservationSucceededHandler>();

            await handler.HandleAsync(
                integrationEvent,
                cancellationToken);

            _logger.LogInformation(
                "Payments processed ReservationSucceeded " +
                "{EventId} for order {OrderId}.",
                integrationEvent.EventId,
                integrationEvent.OrderId);
        }
    }
}
