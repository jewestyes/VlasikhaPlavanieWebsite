namespace VlasikhaPlavanieWebsite.Models
{
	/// <summary>
	/// Модель данных webhook от Альфа-Банка.
	/// </summary>
	public class AlfaWebhook
	{
		public string OrderId { get; set; } = string.Empty;
		public int Amount { get; set; }
		public string Status { get; set; } = string.Empty;
		public bool Success { get; set; }
		public string PaymentId { get; set; } = string.Empty;
		public string Signature { get; set; } = string.Empty;
	}
}
