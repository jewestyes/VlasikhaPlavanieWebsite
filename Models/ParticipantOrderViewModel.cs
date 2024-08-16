namespace VlasikhaPlavanieWebsite.Models
{
	public class ParticipantOrderViewModel
	{
		public int ParticipantId { get; set; }
		public string LastName { get; set; }
		public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public DateTime BirthDate { get; set; }
		public string Gender { get; set; }
		public string CityOrTeam { get; set; }
		public string Rank { get; set; }
		public string Phone { get; set; }
		public string Email { get; set; }

		public int OrderId { get; set; }
		public string OrderNumber { get; set; }
		public decimal Amount { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public OrderStatus Status { get; set; }

		public int DisciplineId { get; set; }
		public string DisciplineName { get; set; }
		public string Distance { get; set; }
		public DateTime StartDate { get; set; }
		public string EntryTime { get; set; }
	}
}
