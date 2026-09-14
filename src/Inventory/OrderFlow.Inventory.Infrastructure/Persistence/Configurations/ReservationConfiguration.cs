using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Inventory.Domain.Entity;

namespace OrderFlow.Inventory.Infrastructure.Persistence.Configurations
{
    public sealed class ReservationConfiguration
        : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(
            EntityTypeBuilder<Reservation> builder
        )
        {
            builder.ToTable(
                "reservations",
                tableBuilder =>
                {
                    tableBuilder.HasCheckConstraint(
                        "ck_reservations_quantity_positive",
                        "quantity > 0");
                });

            builder.HasKey(reservation => reservation.Id);

            builder.Property(reservation => reservation.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.Property(reservation => reservation.ReservationId)
                .HasColumnName("reservation_id")
                .IsRequired();

            builder.Property(reservation => reservation.OrderId)
                .HasColumnName("order_id")
                .IsRequired();

            builder.Property(reservation => reservation.Sku)
                .HasColumnName("sku")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(reservation => reservation.Quantity)
                .HasColumnName("quantity")
                .IsRequired();

            builder.Property(reservation => reservation.Status)
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

            builder.HasIndex(reservation => reservation.ReservationId);

            builder.HasIndex(reservation => reservation.OrderId);

            builder.HasIndex(reservation => new
                {
                    reservation.OrderId,
                    reservation.Sku
                })
                .IsUnique();
        }
    }
}
