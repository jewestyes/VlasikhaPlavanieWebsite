namespace VlasikhaPlavanieWebsite.Models
{
    public class CompetitionDiscipline
    {
        public int Id { get; set; }
        public int CompetitionId { get; set; }
        public string Name { get; set; }
        public string DistancesJson { get; set; }
        public Competition Competition { get; set; }
    }
}
