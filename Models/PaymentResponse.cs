public class PaymentResponse
{
    public int Amount { get; set; }
    public string OrderId { get; set; }
    public string PaymentId { get; set; }
    public string PaymentURL { get; set; }
    public string ErrorCode { get; set; }
    public string TerminalKey { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }
    public bool Success { get; set; }
}
