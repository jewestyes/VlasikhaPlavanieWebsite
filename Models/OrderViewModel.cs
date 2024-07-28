namespace VlasikhaPlavanieWebsite.Models
{
    public class OrderViewModel
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
    }
}
