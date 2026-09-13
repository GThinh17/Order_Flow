using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Orders.Domain.Entity;

namespace OrderFlow.Orders.Infrastructure.Persistence.Configuration
{
    public sealed class OrderConfiguration
        : IEntityTypeConfiguration<Order>
    {
        public void Configure(
            EntityTypeBuilder<Order> builder)
        {
            builder.ToTable(
                "orders",
                tableBuilder =>
                    tableBuilder.HasCheckConstraint(
                        "ck_orders_status",
                        "status IN ('Pending', 'Reserving', 'Charging', 'Confirmed', 'Cancelled')"));

            builder.HasKey(order => order.Id);

            builder.Property(order => order.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.Property(order => order.CustomerId)
                .HasColumnName("customer_id")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(order => order.TotalAmount)
                .HasColumnName("total_amount")
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Property(order => order.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(order => order.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(order => order.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            builder.HasMany(order => order.Lines)
                .WithOne()
                .HasForeignKey("OrderId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(order => order.Lines)
                .UsePropertyAccessMode(
                    PropertyAccessMode.Field
                );
        }
    }
}
