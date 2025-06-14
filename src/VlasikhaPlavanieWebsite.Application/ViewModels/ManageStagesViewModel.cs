using VlasikhaPlavanieWebsite.Models;

namespace VlasikhaPlavanieWebsite.ViewModels
{
    public class ManageCompetitionsViewModel
    {
        public Competition NewCompetition { get; set; }
        public List<string> SelectedDisciplines { get; set; } = new();
        public List<Competition> Competitions { get; set; }
        public Dictionary<string, string> DisciplineDistances { get; set; } = new();
    }
}