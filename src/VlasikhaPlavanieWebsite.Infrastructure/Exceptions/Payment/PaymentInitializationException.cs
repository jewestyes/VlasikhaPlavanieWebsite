namespace VlasikhaPlavanieWebsite.Infrastructure.Exceptions.Payment
{
    public class PaymentInitializationException : Exception
    {
        public string ErrorCode { get; }
        public string PaymentMessage { get; }

        public PaymentInitializationException(string errorCode, string message)
            : base($"Ошибка инициализации платежа: {errorCode} — {message}")
        {
            ErrorCode = errorCode;
            PaymentMessage = message;
        }
    }
}