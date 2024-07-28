namespace VlasikhaPlavanieWebsite.Models
{
    public class PaymentRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string OrderNumber { get; set; }
        public int Amount { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class PaymentResponse
    {
        public string OrderId { get; set; }
        public string FormUrl { get; set; }
    }
}