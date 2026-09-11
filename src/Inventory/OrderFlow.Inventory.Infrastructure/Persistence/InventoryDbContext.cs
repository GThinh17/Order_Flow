using Microsoft.EntityFrameworkCore;

namespace OrderFlow.Inventory.Infrastructure.Persistence;

public sealed class InventoryDbContext(
    DbContextOptions<InventoryDbContext> options)
    : DbContext(options);
