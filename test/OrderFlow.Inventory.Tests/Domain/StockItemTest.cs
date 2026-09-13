using OrderFlow.Inventory.Domain.Entity;

namespace OrderFlow.Inventory.Tests.Domain;

public sealed class StockItemTests
{
    [Fact]
    public void Create_WithValidData_CreatesStockItem()
    {
        var stockItem = StockItem.Create(
            "WIDGET-01",
            10);

        Assert.Equal("WIDGET-01", stockItem.Sku);
        Assert.Equal(10, stockItem.QuantityOnHand);
        Assert.Equal(0, stockItem.QuantityReserved);
        Assert.Equal(10, stockItem.Available);
    }

    [Fact]
    public void AdjustOnHand_WithPositiveQuantity_AddsStock()
    {
        var stockItem = StockItem.Create(
            "WIDGET-01",
            10);

        stockItem.AdjustOnHand(20);

        Assert.Equal(30, stockItem.QuantityOnHand);
        Assert.Equal(30, stockItem.Available);
    }

    [Fact]
    public void AdjustOnHand_BelowZero_ThrowsException()
    {
        var stockItem = StockItem.Create(
            "WIDGET-01",
            10);

        Assert.Throws<InvalidOperationException>(
            () => stockItem.AdjustOnHand(-11));
    }
}