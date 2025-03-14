namespace VlasikhaPlavanieWebsite.Models
{
    public class RegistrationViewModel
    {
        public Dictionary<string, List<string>> DisciplineOptions { get; set; }

        public List<Participant> Participants { get; set; } = new List<Participant> { new Participant() };

        public RegistrationStage Stage { get; set; }

		public DateTime? CompetitionDate { get; set; }
		public string CompetitionAddress { get; set; }
	}
}