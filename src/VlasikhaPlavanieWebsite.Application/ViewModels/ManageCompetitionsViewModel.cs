using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using VlasikhaPlavanieWebsite.Models;

namespace VlasikhaPlavanieWebsite.ViewModels
{
	public class ManageCompetitionsViewModel
    {
        public Competition NewCompetition { get; set; }

		public IFormFile? ImageFile { get; set; }

		public IFormFile? RulesFile { get; set; }

		public IFormFile? RegulationFile { get; set; }

        public List<string> SelectedDisciplines { get; set; } = new();

        public List<Competition> Competitions { get; set; }

        public Dictionary<string, string> DisciplineDistances { get; set; } = new();
	}
}