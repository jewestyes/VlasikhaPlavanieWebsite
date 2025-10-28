namespace VlasikhaPlavanieWebsite.Models
{
	/// <summary>
	/// Модель query-параметров callback Альфа-Банка (GET).
	/// </summary>
	public sealed class AlfaCallbackQuery
	{
		public string mdOrder { get; set; } = string.Empty;
		public string orderNumber { get; set; } = string.Empty;
		public string? checksum { get; set; }
		public string operation { get; set; } = string.Empty;
		public string status { get; set; } = string.Empty;

		// Доп. параметры, которые Альфа может присылать (опциональны):
		public string? callbackCreationDate { get; set; }
		public string? bindingId { get; set; }
		public string? clientId { get; set; }
		public string? enabled { get; set; }
		public string? operationRefundedAmount { get; set; }
		public string? operationRefundedAmountFormatted { get; set; }
		public string? amount { get; set; }
	}
}
