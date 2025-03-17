using VlasikhaPlavanieWebsite.Models;

namespace VlasikhaPlavanieWebsite.ViewModels
{
    public class RegistrationViewModel
    {
        public Dictionary<string, List<string>> DisciplineOptions { get; set; } = new Dictionary<string, List<string>>();

        public List<Participant> Participants { get; set; } = new List<Participant> { new Participant() };

        public RegistrationStage Stage { get; set; }

        public DateTime? CompetitionDate { get; set; } = DateTime.UtcNow.AddHours(3);
        public string? CompetitionAddress { get; set; } = "";
    }
}