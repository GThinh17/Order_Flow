using System.Text.Json;
using Microsoft.Extensions.Configuration;
using OrderFlow.Contracts.IntegrationEvents.Orders;
using OrderFlow.Orders.Application.Messaging;

namespace OrderFlow.Orders.Infrastructure.Persistence.Repositories
{
    public sealed class OutboxWriter : IOutboxWriter
    {
        private static readonly JsonSerializerOptions
            JsonOptions =
                new(JsonSerializerDefaults.Web);

        private readonly OrdersDbContext _dbContext;
        private readonly string _topic;

        public OutboxWriter(
            OrdersDbContext dbContext,
            IConfiguration configuration)
        {
            _dbContext = dbContext;

            _topic = configuration["Pulsar:Topic"]
                ?? throw new InvalidOperationException(
                    "Pulsar topic is not configured.");
        }

        public void Add(OrderPlaced orderPlaced)
        {
            var payload = JsonSerializer.Serialize(
                orderPlaced,
                JsonOptions);

            var message = new OutboxMessage(
                orderPlaced.EventId,
                nameof(OrderPlaced),
                _topic,
                orderPlaced.OrderId.ToString(),
                payload,
                orderPlaced.Timestamp);

            _dbContext.OutboxMessages.Add(message);
        }
    }
}