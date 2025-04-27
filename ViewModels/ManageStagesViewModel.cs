using VlasikhaPlavanieWebsite.Models;

namespace VlasikhaPlavanieWebsite.ViewModels
{
    public class ManageStagesViewModel
    {
        public RegistrationStage NewStage { get; set; }
        public List<string> SelectedDisciplines { get; set; } = new();
        public List<RegistrationStage> Stages { get; set; }
        public Dictionary<string, string> DisciplineDistances { get; set; } = new();
    }
}