using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Payments.Domain.Entity;

namespace OrderFlow.Payments.Infrastructure.Persistence.Configurations
{
    public sealed class PaymentConfiguration
        : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable(
                "payments",
                tableBuilder =>
                {
                    tableBuilder.HasCheckConstraint(
                        "ck_payments_amount_positive",
                        "amount > 0"
                    );
                });

            builder.HasKey(payment => payment.Id);

            builder.Property(payment => payment.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.Property(payment => payment.OrderId)
                .HasColumnName("order_id")
                .IsRequired();

            builder.Property(payment => payment.Amount)
                .HasColumnName("amount")
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Property(payment => payment.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(payment => payment.FailureReason)
                .HasColumnName("failure_reason")
                .HasMaxLength(500);

            builder.Property(payment => payment.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.HasIndex(payment => payment.OrderId)
                .IsUnique();
        }
    }
}