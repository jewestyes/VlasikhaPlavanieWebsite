namespace VlasikhaPlavanieWebsite.Models
{
    public class ManageStagesViewModel
    {
        public List<RegistrationStage> Stages { get; set; }
        public RegistrationStage NewStage { get; set; }
		public List<string> SelectedDisciplines { get; set; } = new();
		public Dictionary<string, string> DisciplineDistances { get; set; } = new();
	}
}
