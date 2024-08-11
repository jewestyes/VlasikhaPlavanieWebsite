namespace VlasikhaPlavanieWebsite.Models
{
    public class TinkoffWebhookModel
    {
        public string TerminalKey { get; set; }
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }  
        public string PaymentId { get; set; }
        public string ErrorCode { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
    }

}
