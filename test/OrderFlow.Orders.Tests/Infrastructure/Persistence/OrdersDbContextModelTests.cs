using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using OrderFlow.Orders.Domain.Entity;
using OrderFlow.Orders.Infrastructure.Persistence;
using OrderFlow.Orders.Infrastructure.Persistence.Repositories;

namespace OrderFlow.Orders.Tests.Infrastructure.Persistence;

public sealed class OrdersDbContextModelTests
{
    private readonly OrdersDbContext _context = new(
        new DbContextOptionsBuilder<OrdersDbContext>()
            .UseNpgsql(
                "Host=localhost;Database=orderflow_orders;Username=test;Password=test")
            .Options);

    [Fact]
    public void OutboxMessage_UsesExpectedColumnsAndNullablePublishedAt()
    {
        var entityType = _context.Model.FindEntityType(typeof(OutboxMessage));
        Assert.NotNull(entityType);

        var table = StoreObjectIdentifier.Table(
            "out_messages",
            OrdersDbContext.SchemaName);

        AssertColumn(entityType, nameof(OutboxMessage.EventId), "event_id", table);
        AssertColumn(entityType, nameof(OutboxMessage.PartitionKey), "partition_key", table);
        AssertColumn(entityType, nameof(OutboxMessage.CreatedAt), "created_at", table);
        AssertColumn(entityType, nameof(OutboxMessage.PublishedAt), "published_at", table);

        Assert.True(entityType.FindProperty(nameof(OutboxMessage.PublishedAt))!.IsNullable);
    }

    [Fact]
    public void Order_UsesConventionalTimestampColumns()
    {
        var entityType = _context.Model.FindEntityType(typeof(Order));
        Assert.NotNull(entityType);

        var table = StoreObjectIdentifier.Table(
            "orders",
            OrdersDbContext.SchemaName);

        AssertColumn(entityType, nameof(Order.CreatedAt), "created_at", table);
        AssertColumn(entityType, nameof(Order.UpdatedAt), "updated_at", table);
    }

    [Fact]
    public void OrderLine_UsesIntegerQuantityAndMappedSku()
    {
        var entityType = _context.Model.FindEntityType(typeof(OrderLine));
        Assert.NotNull(entityType);

        var table = StoreObjectIdentifier.Table(
            "order_lines",
            OrdersDbContext.SchemaName);

        var quantity = entityType.FindProperty(nameof(OrderLine.Quantity));
        Assert.NotNull(quantity);
        Assert.Equal(typeof(int), quantity.ClrType);
        Assert.Equal("integer", quantity.GetColumnType());
        Assert.Equal("quantity", quantity.GetColumnName(table));

        var sku = entityType.FindProperty(nameof(OrderLine.Sku));
        Assert.NotNull(sku);
        Assert.Equal(50, sku.GetMaxLength());
        Assert.Equal("sku", sku.GetColumnName(table));
    }

    [Fact]
    public void InboxMessage_UsesEventIdAsPrimaryKey()
    {
        var entityType = _context.Model.FindEntityType(
            typeof(InboxMessage));
        Assert.NotNull(entityType);

        var primaryKey = entityType.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Equal(
            nameof(InboxMessage.EventId),
            Assert.Single(primaryKey.Properties).Name);
    }

    [Fact]
    public void OrderSagaState_UsesOrderIdAsPrimaryKeyAndForeignKey()
    {
        var entityType = _context.Model.FindEntityType(
            typeof(OrderSagaState));
        Assert.NotNull(entityType);

        var primaryKey = entityType.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Equal(
            nameof(OrderSagaState.OrderId),
            Assert.Single(primaryKey.Properties).Name);

        var foreignKey = Assert.Single(entityType.GetForeignKeys());
        Assert.Equal(
            nameof(OrderSagaState.OrderId),
            Assert.Single(foreignKey.Properties).Name);
    }

    private static void AssertColumn(
        IReadOnlyEntityType entityType,
        string propertyName,
        string expectedColumnName,
        StoreObjectIdentifier table)
    {
        var property = entityType.FindProperty(propertyName);
        Assert.NotNull(property);
        Assert.Equal(expectedColumnName, property.GetColumnName(table));
    }
}
