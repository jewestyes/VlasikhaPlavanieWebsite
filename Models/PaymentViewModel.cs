namespace VlasikhaPlavanieWebsite.Models
{
	public class PaymentViewModel
	{
		public string OrderId { get; set; } 
		public decimal Amount { get; set; }
		public string Description { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
	}

}
