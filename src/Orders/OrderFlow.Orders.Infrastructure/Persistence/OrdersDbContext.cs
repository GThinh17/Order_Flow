using Microsoft.EntityFrameworkCore;
using OrderFlow.Orders.Application.Abstractions.Persistence;
using OrderFlow.Orders.Domain.Entity;

namespace OrderFlow.Orders.Infrastructure.Persistence;

public sealed class OrdersDbContext
    : DbContext, IUnitOfWork
{
    public const string SchemaName = "orderflow_orders";

    public OrdersDbContext(
        DbContextOptions<OrdersDbContext> options)
            : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(OrdersDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
