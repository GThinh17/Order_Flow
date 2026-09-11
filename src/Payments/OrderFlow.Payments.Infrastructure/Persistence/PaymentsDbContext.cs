using Microsoft.EntityFrameworkCore;

namespace OrderFlow.Payments.Infrastructure.Persistence;

public sealed class PaymentsDbContext(
    DbContextOptions<PaymentsDbContext> options)
    : DbContext(options);
