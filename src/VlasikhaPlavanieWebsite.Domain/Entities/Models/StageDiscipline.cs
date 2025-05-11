namespace VlasikhaPlavanieWebsite.Models
{
    public class StageDiscipline
    {
        public int Id { get; set; }
        public int StageId { get; set; }
        public string Name { get; set; }
        public string DistancesJson { get; set; }
        public RegistrationStage Stage { get; set; }
    }
}
