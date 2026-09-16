using Microsoft.EntityFrameworkCore;
using OrderFlow.Payments.Domain.Entity;
using OrderFlow.Payments.Infrastructure.Persistence.Models;

namespace OrderFlow.Payments.Infrastructure.Persistence
{
    public sealed class PaymentsDbContext
        : DbContext
    {
        public const string SchemaName = "orderflow_payments";

        public PaymentsDbContext(
            DbContextOptions<PaymentsDbContext> options)
                : base(options)
        {
        }

        public DbSet<Payment> Payments => Set<Payment>();

        public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(SchemaName);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(PaymentsDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }

    }
}
