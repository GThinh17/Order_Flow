using Microsoft.EntityFrameworkCore;

namespace OrderFlow.Orders.Infrastructure.Persistence;

public sealed class OrdersDbContext(
    DbContextOptions<OrdersDbContext> options)
    : DbContext(options);
