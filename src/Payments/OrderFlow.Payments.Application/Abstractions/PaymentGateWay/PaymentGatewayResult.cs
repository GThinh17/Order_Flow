namespace OrderFlow.Payments.Application.Abstractions.PaymentGateWay
{
    public sealed record PaymentGateWayResult
    {
        private PaymentGateWayResult(
            bool isSuccessful,
            string? failureReason
        )
        {
            IsSuccessful = isSuccessful;
            FailureReason = failureReason;
        }

        public bool IsSuccessful { get; }
        public string? FailureReason { get; }

        public static PaymentGateWayResult Success()
        {
            return new PaymentGateWayResult(
                isSuccessful: true,
                failureReason: null
            );
        }

        public static PaymentGateWayResult Failure(string failureReason)
        {
            if (string.IsNullOrWhiteSpace(failureReason))
            {
                throw new ArgumentException(
                    "Failure is require reason",
                    nameof(failureReason));
            }

            return new PaymentGateWayResult(
                isSuccessful: false,
                failureReason: failureReason.Trim()
            );
        }
    }

}