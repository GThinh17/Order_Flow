namespace OrderFlow.Orders.Domain.Constants
{
    public static class DomainMessageError
    {
        public const string RequiredSkuError = "SKU is required";

        public const string RequiredCustomerIdError = "Customer is required";

        public const string InvalidCustomerIdError = "Customer is longer than 100 characters";

        public const string AcceptableOrderLineError = "An order must at least one line";

        public const string InvalidSkuLengthError = "SKU cannot longer than 50 characters";

        public const string InvalidNumberError = "{0} must be greater than zero.";

        public const string InvalidUnitPrice = "Unit Price cannot have more than two decimal places";
    }
}