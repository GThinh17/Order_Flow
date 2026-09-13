using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Inventory.Domain.Entity;

namespace OrderFlow.Inventory.Application.Abstractions.Persistence.Configurations
{
    public sealed class StockItemConfiguration
        : IEntityTypeConfiguration<StockItem>
    {
        public void Configure(
            EntityTypeBuilder<StockItem> builder)
        {
            builder.ToTable(
                "stock_items",
                tableBuilder =>
                {
                    tableBuilder.HasCheckConstraint(
                        "ck_stock_on_hand_non_nagative",
                        "quantity_on_hand >= 0");

                    tableBuilder.HasCheckConstraint(
                        "ck_stock_reserved_non_nagative",
                        "quantity_reserved >= 0");

                    tableBuilder.HasCheckConstraint(
                        "ck_stock_reserved_not_exceed_on_hanh",
                        "quantity_reserved <= quantity_on_hand");
                });

            builder.HasKey(stock => stock.Sku);

            builder.Property(stock => stock.Sku)
                .HasColumnName("sku")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(stock => stock.QuantityOnHand)
                .HasColumnName("quantity_on_hand")
                .IsRequired();

            builder.Property(stock => stock.QuantityReserved)
                .HasColumnName("quantity_reserved")
                .IsRequired();

            builder.Ignore(stock => stock.Available);
        }
    }
}