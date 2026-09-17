using OrderFlow.Inventory.Domain.Entity;

namespace OrderFlow.Inventory.Tests.Domain;

public sealed class StockItemTests
{
    [Fact]
    public void Create_WithValidData_CreatesStockItem()
    {
        // Arrange
        const string sku = "WIDGET-01";
        const int quantityOnHand = 10;

        // Act
        var stockItem = StockItem.Create(
            sku,
            quantityOnHand);

        // Assert
        Assert.Equal("WIDGET-01", stockItem.Sku);
        Assert.Equal(10, stockItem.QuantityOnHand);
        Assert.Equal(0, stockItem.QuantityReserved);
        Assert.Equal(10, stockItem.Available);
    }

    [Fact]
    public void AdjustOnHand_WithPositiveQuantity_AddsStock()
    {
        // Arrange
        var stockItem = StockItem.Create(
            "WIDGET-01",
            10);

        // Act
        stockItem.AdjustOnHand(20);

        // Assert
        Assert.Equal(30, stockItem.QuantityOnHand);
        Assert.Equal(30, stockItem.Available);
    }

    [Fact]
    public void AdjustOnHand_BelowZero_ThrowsException()
    {
        // Arrange
        var stockItem = StockItem.Create(
            "WIDGET-01",
            10);

        // Act
        var exception = Record.Exception(
            () => stockItem.AdjustOnHand(-11));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void TryReserve_WhenStockIsAvailable_ReservesStock()
    {
        // Arrange
        var stock = StockItem.Create("WIDGET-01", 10);

        // Act
        var succeeded = stock.TryReserve(2);

        // Assert
        Assert.True(succeeded);
        Assert.Equal(2, stock.QuantityReserved);
        Assert.Equal(8, stock.Available);
    }

    [Fact]
    public void TryReserve_WhenStockIsInsufficient_DoesNotChangeStock()
    {
        // Arrange
        var stock = StockItem.Create("WIDGET-01", 1);

        // Act
        var succeeded = stock.TryReserve(2);

        // Assert
        Assert.False(succeeded);
        Assert.Equal(0, stock.QuantityReserved);
        Assert.Equal(1, stock.Available);
    }

    [Fact]
    public void Release_RestoresAvailableStock()
    {
        // Arrange
        var stock = StockItem.Create("WIDGET-01", 10);
        stock.TryReserve(2);

        // Act
        stock.Release(2);

        // Assert
        Assert.Equal(10, stock.QuantityOnHand);
        Assert.Equal(0, stock.QuantityReserved);
        Assert.Equal(10, stock.Available);
    }

    [Fact]
    public void Consume_PermanentlyReducesStock()
    {
        // Arrange
        var stock = StockItem.Create("WIDGET-01", 10);
        stock.TryReserve(2);

        // Act
        stock.Consume(2);

        // Assert
        Assert.Equal(8, stock.QuantityOnHand);
        Assert.Equal(0, stock.QuantityReserved);
        Assert.Equal(8, stock.Available);
    }
}
