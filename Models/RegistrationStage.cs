namespace VlasikhaPlavanieWebsite.Models
{
    public class RegistrationStage
    {
        public int Id { get; set; }
        public string StageName { get; set; }
        public DateTime RegistrationStartDate { get; set; } = DateTime.UtcNow.AddHours(3);
        public DateTime? RegistrationEndDate { get; set; }
        public bool IsOpen { get; set; }
    }
}
