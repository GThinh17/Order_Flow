using OrderFlow.Orders.Domain.Constants;
namespace OrderFlow.Orders.Domain.Entity
{
    public sealed class OrderLine
    {
        private OrderLine()
        {

        }

        public long Id { get; set; }

        public string Sku { get; set; } = string.Empty;

        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalAmount => Quantity * UnitPrice;

        public static OrderLine Create(
            string sku,
            int quantity,
            decimal unitPrice)
        {
            if (string.IsNullOrWhiteSpace(sku))
            {
                throw new ArgumentException(
                    DomainMessageError.RequiredSkuError,
                    nameof(sku));
            }

            var validSku = sku.Trim();

            if (validSku.Length > 50)
            {
                throw new ArgumentException
                (
                    DomainMessageError.InvalidSkuLengthError,
                    nameof(sku));
            }

            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(quantity),
                    string.Format(
                        DomainMessageError.InvalidNumberError,
                        nameof(quantity)));
            }

            if (unitPrice <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(unitPrice),
                    string.Format(
                        DomainMessageError.InvalidNumberError,
                        nameof(unitPrice)));
            }

            if (decimal.Round(unitPrice, 2) != unitPrice)
            {
                throw new ArgumentException(
                    DomainMessageError.InvalidUnitPrice,
                    nameof(unitPrice));
            }

            return new OrderLine
            {
                Sku = validSku,
                Quantity = quantity,
                UnitPrice = unitPrice
            };
        }
    }
}