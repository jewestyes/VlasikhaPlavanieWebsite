namespace VlasikhaPlavanieWebsite.Models
{
	/// <summary>
	/// Ответ регистрации заказа у Альфа-Банка.
	/// </summary>
	public class AlfaRegisterResponse
	{
		public string OrderId { get; set; } = string.Empty;
		public string FormUrl { get; set; } = string.Empty;
		public string ErrorCode { get; set; } = string.Empty;
		public string ErrorMessage { get; set; } = string.Empty;
	}
}
