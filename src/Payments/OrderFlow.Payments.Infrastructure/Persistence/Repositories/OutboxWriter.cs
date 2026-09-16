using System.Text.Json;
using Microsoft.Extensions.Configuration;
using OrderFlow.Contracts.IntegrationEvents.Payments;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Infrastructure.Persistence.Models;

namespace OrderFlow.Payments.Infrastructure.Persistence.Repositories
{
    public sealed class OutboxWriter : IOutboxWriter
    {
        private static readonly JsonSerializerOptions JsonOptions =
            new(JsonSerializerDefaults.Web);
        private readonly PaymentsDbContext _dbContext;
        private readonly string _topic;

        public OutboxWriter(
            PaymentsDbContext dbContext,
            IConfiguration configuration)
        {
            _dbContext = dbContext;

            _topic = configuration["Pulsar:Topic"]
                ?? throw new InvalidOperationException(
                    "Pulsar topic is not configured.");
        }
        public void Add(PaymentSucceeded integrationEvent)
        {
            AddMessage(
                integrationEvent.EventId,
                nameof(PaymentSucceeded),
                integrationEvent.OrderId,
                integrationEvent.Timestamp,
                integrationEvent);
        }

        public void Add(PaymentFailed integrationEvent)
        {
            AddMessage(
                integrationEvent.EventId,
                nameof(PaymentFailed),
                integrationEvent.OrderId,
                integrationEvent.Timestamp,
                integrationEvent);
        }
        private void AddMessage<TEvent>(
        Guid eventId,
        string eventType,
        Guid orderId,
        DateTimeOffset createdAt,
        TEvent integrationEvent)
        {
            var payload = JsonSerializer.Serialize(
                integrationEvent,
                JsonOptions);

            _dbContext.OutboxMessages.Add(
                new OutboxMessage(
                    eventId,
                    eventType,
                    _topic,
                    orderId.ToString("D"),
                    payload,
                    createdAt));
        }
    }
}