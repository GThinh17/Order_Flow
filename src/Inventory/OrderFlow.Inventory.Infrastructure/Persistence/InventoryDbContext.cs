using Microsoft.EntityFrameworkCore;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Entity;

namespace OrderFlow.Inventory.Infrastructure.Persistence
{
    public sealed class InventoryDbContext
        : DbContext, IUnitOfWork
    {
        public const string SchemaName =
            "orderflow_inventory";

        public InventoryDbContext(
            DbContextOptions<InventoryDbContext> options)
            : base(options)
        {
        }

        public DbSet<StockItem> StockItems => Set<StockItem>();

        public DbSet<Reservation> Reservations => Set<Reservation>();

        public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(SchemaName);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(InventoryDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}


