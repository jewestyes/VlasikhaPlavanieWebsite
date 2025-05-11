namespace VlasikhaPlavanieWebsite.Infrastructure.Exceptions.Payment
{
    public class TokenGenerationException : Exception
    {
        public string OrderId { get; }
        public TokenGenerationException(string orderId, Exception inner)
            : base($"Ошибка генерации токена для OrderId={orderId}.", inner)
        {
            OrderId = orderId;
        }
    }
}