namespace VlasikhaPlavanieWebsite.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public decimal Amount { get; set; }
        public List<Participant> Participants { get; set; }
    }
}
