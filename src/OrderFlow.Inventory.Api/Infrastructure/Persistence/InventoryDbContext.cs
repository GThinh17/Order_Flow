using Microsoft.EntityFrameworkCore;

namespace OrderFlow.Inventory.Api.Infrastructure.Persistence;

internal sealed class InventoryDbContext(
    DbContextOptions<InventoryDbContext> options)
    : DbContext(options);