namespace VlasikhaPlavanieWebsite.Infrastructure.Exceptions.Payment
{
    public class PaymentAmountMismatchException : Exception
    {
        public string OrderId { get; }
        public decimal Expected { get; }
        public decimal Provided { get; }

        public PaymentAmountMismatchException(string orderId, decimal expected, decimal provided)
            : base($"Несоответствие суммы для OrderId={orderId}: ожидалось {expected}, получили {provided}.")
        {
            OrderId = orderId;
            Expected = expected;
            Provided = provided;
        }
    }
}