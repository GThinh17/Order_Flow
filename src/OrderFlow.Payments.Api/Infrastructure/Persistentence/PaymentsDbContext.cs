using Microsoft.EntityFrameworkCore;

namespace OrderFlow.Payments.Api.Infrastructure.Persistence;

internal sealed class PaymentsDbContext(
    DbContextOptions<PaymentsDbContext> options)
    : DbContext(options);