namespace VlasikhaPlavanieWebsite.Models
{
    public enum OrderStatus
    {
        Pending,
        Processing,
        Paid,
        Failed,
        Cancelled,
        Refunded
    }

    public class Order
    {
        public int Id { get; set; }
        public int CompetitionId { get; set; }
        public string OrderNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public List<Participant> Participants { get; set; }
        public Competition Competition { get; set; }
    }
}
