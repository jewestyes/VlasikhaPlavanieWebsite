namespace VlasikhaPlavanieWebsite.Infrastructure.Exceptions.Payment
{
    public class ParticipantNotFoundException : Exception
    {
        public string OrderId { get; }
        public ParticipantNotFoundException(string orderId)
            : base($"Не удалось найти участников для OrderId: {orderId}.")
        {
            OrderId = orderId;
        }
    }
}