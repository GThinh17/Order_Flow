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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(SchemaName);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(InventoryDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}


