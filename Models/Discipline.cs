namespace VlasikhaPlavanieWebsite.Models
{
	public class Discipline
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Distance { get; set; }
		public DateTime StartDate { get; set; } = DateTime.Now;
		public string EntryTime { get; set; }

		public int ParticipantId { get; set; } 
	}
}
