namespace VlasikhaPlavanieWebsite.Models
{
    public class Competition
    {
        public int Id { get; set; }
        public string Name { get; set; }
		public string Address { get; set; } 
        public DateTime RegistrationStartDate { get; set; } = DateTime.UtcNow.AddHours(3);
        public DateTime? RegistrationEndDate { get; set; }
		public DateTime? CompetitionStartDate { get; set; } = DateTime.UtcNow.AddHours(3);
		public bool IsOpen { get; set; }
	}
}
