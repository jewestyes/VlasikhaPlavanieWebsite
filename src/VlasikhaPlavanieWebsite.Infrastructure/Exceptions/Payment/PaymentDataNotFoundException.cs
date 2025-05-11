namespace VlasikhaPlavanieWebsite.Infrastructure.Exceptions.Payment
{
    public class PaymentDataNotFoundException : Exception
    {
        public string OrderId { get; }
        public PaymentDataNotFoundException(string orderId)
            : base($"Не удалось найти данные для оплаты по OrderId: {orderId}.")
        {
            OrderId = orderId;
        }
    }
}