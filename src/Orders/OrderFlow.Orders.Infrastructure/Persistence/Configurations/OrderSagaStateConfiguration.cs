using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Orders.Domain.Entity;

namespace OrderFlow.Orders.Infrastructure.Persistence.Configuration;

public sealed class OrderSagaStateConfiguration
    : IEntityTypeConfiguration<OrderSagaState>
{
    public void Configure(
        EntityTypeBuilder<OrderSagaState> builder)
    {
        builder.ToTable("order_saga_state");

        builder.HasKey(state => state.OrderId);

        builder.Property(state => state.OrderId)
            .HasColumnName("order_id")
            .ValueGeneratedNever();

        builder.Property(state => state.ReservationCompleted)
            .HasColumnName("reservation_completed")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(state => state.PaymentCompleted)
            .HasColumnName("payment_completed")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(state => state.LastProcessedEventId)
            .HasColumnName("last_processed_event_id");

        builder.HasOne<Order>()
            .WithOne()
            .HasForeignKey<OrderSagaState>(
                state => state.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
