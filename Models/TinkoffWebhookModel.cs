namespace VlasikhaPlavanieWebsite.Models
{
    public class TinkoffWebhookModel
    {
        public string TerminalKey { get; set; }
        public string OrderId { get; set; }
        public bool Success { get; set; }
        public string Status { get; set; }
        public long PaymentId { get; set; }
        public string ErrorCode { get; set; }
        public int Amount { get; set; }
        public long CardId { get; set; }
        public string Pan { get; set; }
        public string ExpDate { get; set; }
        public string Token { get; set; }
    }
}
