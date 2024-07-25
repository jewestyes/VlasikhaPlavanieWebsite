namespace VlasikhaPlavanieWebsite.Models
{
    public class Participant
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; }
        public string CityOrTeam { get; set; }
        public string Rank { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public List<Discipline> Disciplines { get; set; } = new List<Discipline> { new Discipline() };
    }
}
