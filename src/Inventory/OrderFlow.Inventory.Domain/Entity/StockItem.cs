namespace OrderFlow.Inventory.Domain.Entity
{
    public sealed class StockItem
    {
        private StockItem()
        {
        }

        private StockItem(
        string sku,
        int quantityOnHand)
        {
            Sku = sku;
            QuantityOnHand = quantityOnHand;
            QuantityReserved = 0;

        }
        public string Sku { get; private set; } = string.Empty;
        public int QuantityOnHand { get; private set; }
        public int QuantityReserved { get; private set; }
        public int Available =>
            QuantityOnHand - QuantityReserved;

        public static StockItem Create(
            string sku,
            int quantityOnHand)
        {
            if (string.IsNullOrWhiteSpace(sku))
            {
                throw new ArgumentException(
                    "SKU is required",
                    nameof(sku));
            }

            string validSku = sku.Trim();

            if (validSku.Length > 50)
            {
                throw new ArgumentException(
                    "SKU cannot exceed 50 characters",
                    nameof(sku));
            }

            if (quantityOnHand < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(quantityOnHand),
                    "Quantity on hand cannot be nagative");
            }

            return new StockItem(
                validSku,
                quantityOnHand
            );
        }

        public void AdjustOnHand(int quantity)
        {
            if (quantity is 0)
            {
                throw new ArgumentException(
                    "Adjustment quantity cannot be zero",
                    nameof(quantity));
            }

            var adjustedQuantity =
                checked(QuantityOnHand + quantity);

            if (adjustedQuantity < QuantityReserved)
            {
                throw new InvalidOperationException(
                    "Quantity on hand cannot lowwer than reserved quantity ");
            }

            QuantityOnHand = adjustedQuantity;
        }
    }
}