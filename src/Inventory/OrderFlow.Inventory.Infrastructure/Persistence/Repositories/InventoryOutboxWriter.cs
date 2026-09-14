using System.Text.Json;
using Microsoft.Extensions.Configuration;
using OrderFlow.Contracts.IntegrationEvents.Inventory;
using OrderFlow.Inventory.Application.Abstractions.Persistence;

namespace OrderFlow.Inventory.Infrastructure.Persistence.Repositories;

public sealed class InventoryOutboxWriter : IInventoryOutboxWriter
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly InventoryDbContext _dbContext;
    private readonly string _topic;

    public InventoryOutboxWriter(
        InventoryDbContext dbContext,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _topic = configuration["Pulsar:Topic"]
            ?? throw new InvalidOperationException(
                "Pulsar topic is not configured.");
    }

    public void Add(ReservationSucceeded integrationEvent)
    {
        AddMessage(
            integrationEvent.EventId,
            nameof(ReservationSucceeded),
            integrationEvent.OrderId,
            integrationEvent.Timestamp,
            integrationEvent);
    }

    public void Add(ReservationFailed integrationEvent)
    {
        AddMessage(
            integrationEvent.EventId,
            nameof(ReservationFailed),
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
