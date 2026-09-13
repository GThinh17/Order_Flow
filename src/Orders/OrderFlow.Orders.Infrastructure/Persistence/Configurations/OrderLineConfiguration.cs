using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Orders.Domain.Entity;


namespace OrderFlow.Orders.Infrastructure.Persistence.Configuration
{
    public sealed class OrderLineConfiguraton
        : IEntityTypeConfiguration<OrderLine>
    {
        public void Configure(
            EntityTypeBuilder<OrderLine> builder)
        {
            builder.ToTable(
                "order_lines",
                tableBuilder =>
                {
                    tableBuilder.HasCheckConstraint(
                        "ck_order_lines_quantity",
                        "quantity > 0");

                    tableBuilder.HasCheckConstraint(
                        "ck_order_lines_unit_price",
                        "unit_price > 0");
                });

            builder.HasKey(orderLine => orderLine.Id);

            builder.Property(orderLine => orderLine.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property<Guid>("OrderId")
                .HasColumnName("order_id")
                .IsRequired();

            builder.Property(orderLine => orderLine.Sku)
                .HasColumnName("sku")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(orderLine => orderLine.Quantity)
                .HasColumnName("quantity")
                .HasColumnType("integer")
                .IsRequired();

            builder.Property(orderLine => orderLine.UnitPrice)
                .HasColumnName("unit_price")
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Ignore(orderLine =>
                orderLine.TotalAmount);

            builder.HasIndex("OrderId")
                .HasDatabaseName(
                    "ix_order_lines_order_id");
        }
    }
}
