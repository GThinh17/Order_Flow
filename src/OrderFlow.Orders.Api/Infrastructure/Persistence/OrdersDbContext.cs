using Microsoft.EntityFrameworkCore;

namespace OrderFlow.Orders.Api.Infrastructure.Persistence;

internal sealed class OrdersDbContext(
    DbContextOptions<OrdersDbContext> options)
    : DbContext(options);
