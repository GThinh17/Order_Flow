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

    [Fact]
    public void TryReserve_WhenStockIsAvailable_ReservesStock()
    {
        var stock = StockItem.Create("WIDGET-01", 10);

        var succeeded = stock.TryReserve(2);

        Assert.True(succeeded);
        Assert.Equal(2, stock.QuantityReserved);
        Assert.Equal(8, stock.Available);
    }

    [Fact]
    public void TryReserve_WhenStockIsInsufficient_DoesNotChangeStock()
    {
        var stock = StockItem.Create("WIDGET-01", 1);

        var succeeded = stock.TryReserve(2);

        Assert.False(succeeded);
        Assert.Equal(0, stock.QuantityReserved);
        Assert.Equal(1, stock.Available);
    }

    [Fact]
    public void Release_RestoresAvailableStock()
    {
        var stock = StockItem.Create("WIDGET-01", 10);
        stock.TryReserve(2);

        stock.Release(2);

        Assert.Equal(10, stock.QuantityOnHand);
        Assert.Equal(0, stock.QuantityReserved);
        Assert.Equal(10, stock.Available);
    }

    [Fact]
    public void Consume_PermanentlyReducesStock()
    {
        var stock = StockItem.Create("WIDGET-01", 10);
        stock.TryReserve(2);

        stock.Consume(2);

        Assert.Equal(8, stock.QuantityOnHand);
        Assert.Equal(0, stock.QuantityReserved);
        Assert.Equal(8, stock.Available);
    }
}