namespace VlasikhaPlavanieWebsite.Models
{
    public enum OrderStatus
    {
        Pending,     // Ожидание
        Processing,  // В процессе обработки
        Paid,        // Оплачен
        Failed,      // Ошибка оплаты
        Cancelled,   // Отменен
        Refunded     // Возвращен
    }

    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public decimal Amount { get; set; }
        public List<Participant> Participants { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
