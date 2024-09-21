namespace VlasikhaPlavanieWebsite.Models
{
    public class RegistrationViewModel
    {
        public List<Participant> Participants { get; set; } = new List<Participant> { new Participant() };

        public RegistrationStage Stage { get; set; }
    }
}
