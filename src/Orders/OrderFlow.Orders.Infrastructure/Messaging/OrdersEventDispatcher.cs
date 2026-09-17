using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderFlow.Contracts.IntegrationEvents.Inventory;
using OrderFlow.Contracts.IntegrationEvents.Payments;
using OrderFlow.Orders.Application.Handler;

namespace OrderFlow.Orders.Infrastructure.Messaging
{
    public sealed class OrdersEventDispatcher
    : IOrdersEventDispatcher
    {
        private static readonly JsonSerializerOptions JsonOptions =
            new(JsonSerializerDefaults.Web);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OrdersEventDispatcher> _logger;

        public OrdersEventDispatcher(
            IServiceScopeFactory scopeFactory,
            ILogger<OrdersEventDispatcher> logger)
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
                case nameof(ReservationSucceeded):
                    {
                        var integrationEvent =
                            Deserialize<ReservationSucceeded>(payload);

                        var handler = scope.ServiceProvider
                            .GetRequiredService<
                                ReservationSucceededHandler>();

                        await handler.HandleAsync(
                            integrationEvent,
                            cancellationToken);

                        break;
                    }

                case nameof(ReservationFailed):
                    {
                        var integrationEvent =
                            Deserialize<ReservationFailed>(payload);

                        var handler = scope.ServiceProvider
                            .GetRequiredService<
                                ReservationFailedHandler>();

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
                                PaymentSucceededHandler>();

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
                                PaymentFailedHandler>();

                        await handler.HandleAsync(
                            integrationEvent,
                            cancellationToken);

                        break;
                    }

                default:
                    _logger.LogDebug(
                        "Orders ignored event type {EventType}.",
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
