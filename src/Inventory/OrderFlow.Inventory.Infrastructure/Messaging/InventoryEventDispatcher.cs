using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderFlow.Contracts.IntegrationEvents.Orders;
using OrderFlow.Contracts.IntegrationEvents.Payments;
using OrderFlow.Inventory.Application.Handler;

namespace OrderFlow.Inventory.Infrastructure.Messaging
{
    public sealed class InventoryEventDispatcher
    : IInventoryEventDispatcher
    {
        private static readonly JsonSerializerOptions JsonOptions =
            new(JsonSerializerDefaults.Web);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<InventoryEventDispatcher> _logger;

        public InventoryEventDispatcher(
            IServiceScopeFactory scopeFactory,
            ILogger<InventoryEventDispatcher> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task DispatchAsync(
            string eventType,
            string payload,
            CancellationToken cancellationToken)
        {
            await using var scope =
                _scopeFactory.CreateAsyncScope();

            switch (eventType)
            {
                case nameof(OrderPlaced):
                    {
                        var integrationEvent =
                            Deserialize<OrderPlaced>(payload);

                        var handler = scope.ServiceProvider
                            .GetRequiredService<ReserveOrderHandler>();

                        await handler.HandleAsync(
                            integrationEvent,
                            cancellationToken);

                        break;
                    }

                case nameof(PaymentSucceeded):
                    {
                        var integrationEvent =
                            Deserialize<PaymentSucceeded>(payload);

                        var handler = scope.ServiceProvider
                            .GetRequiredService<
                                ConsumeReservationHandler>();

                        await handler.HandleAsync(
                            integrationEvent,
                            cancellationToken);

                        break;
                    }

                case nameof(PaymentFailed):
                    {
                        var integrationEvent =
                            Deserialize<PaymentFailed>(payload);

                        var handler = scope.ServiceProvider
                            .GetRequiredService<
                                ReleaseReservationHandler>();

                        await handler.HandleAsync(
                            integrationEvent,
                            cancellationToken);

                        break;
                    }

                default:
                    _logger.LogDebug(
                        "Inventory ignored event type {EventType}.",
                        eventType);

                    break;
            }
        }

        private static TEvent Deserialize<TEvent>(
            string payload)
        {
            return JsonSerializer.Deserialize<TEvent>(
                    payload,
                    JsonOptions)
                ?? throw new JsonException(
                    $"{typeof(TEvent).Name} payload is null.");
        }
    }
}
