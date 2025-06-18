using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text.Json;
using VlasikhaPlavanieWebsite.Data;
using VlasikhaPlavanieWebsite.Models;
using VlasikhaPlavanieWebsite.ViewModels;

namespace VlasikhaPlavanieWebsite.Infrastructure.Services.Admin
{
	public class CompetitionService : ICompetitionService
	{
		private readonly ApplicationDbContext _applicationDbContext;
		public CompetitionService(ApplicationDbContext applicationDbContext)
		{
			_applicationDbContext = applicationDbContext;
		}

		public async Task ChangeStatusAsync(int id, bool isOpen)
		{
			var competition = await _applicationDbContext.Competitions.FindAsync(id);

			if (competition != null)
			{
				competition.IsOpen = isOpen;

				if (!isOpen)
				{
					competition.RegistrationEndDate = DateTime.UtcNow.AddHours(3);
				}
				else
				{
					competition.RegistrationEndDate = null;
				}
				await _applicationDbContext.SaveChangesAsync();
			}
		}

		public async Task CreateCompetitionAsync(ManageCompetitionsViewModel model)
		{

			model.NewCompetition.IsOpen = false;
			_applicationDbContext.Competitions.Add(model.NewCompetition);
			await _applicationDbContext.SaveChangesAsync();

			await SaveUploadedFileAsync(
				model.ImageFile,
				"images",
				"ImageFilePath",
				model.NewCompetition,
				$"competitions_{model.NewCompetition.Id}_{model.ImageFile?.FileName}"
			);

			await SaveUploadedFileAsync(
				model.RulesFile,
				"Files",
				"RulesFilePath",
				model.NewCompetition,
				$"competitions_{model.NewCompetition.Id}_{model.RulesFile?.FileName}"
			);

			await SaveUploadedFileAsync(
				model.RegulationFile,
				"Files",
				"RegulationFilePath",
				model.NewCompetition,
				$"competitions_{model.NewCompetition.Id}_{model.RegulationFile?.FileName}"
			);

			if (model.SelectedDisciplines != null && model.SelectedDisciplines.Any())
			{
				var competitionDisciplines = new List<CompetitionDiscipline>();

				foreach (var discipline in model.SelectedDisciplines)
				{
					if (model.DisciplineDistances.TryGetValue(discipline, out string distances)
						&& !string.IsNullOrWhiteSpace(distances))
					{
						var distanceList = distances.Split(';')
							.Select(d => d.Trim())
							.Where(d => !string.IsNullOrEmpty(d))
							.ToList();

						competitionDisciplines.Add(new CompetitionDiscipline
						{
							CompetitionId = model.NewCompetition.Id,
							Name = discipline,
							DistancesJson = JsonSerializer.Serialize(distanceList)
						});
					}
				}

				if (competitionDisciplines.Any())
				{
					await _applicationDbContext.CompetitionDisciplines.AddRangeAsync(competitionDisciplines);
					await _applicationDbContext.SaveChangesAsync();
				}
			}
		}
		
		public async Task<List<Competition>> GetAllAsync()
		{
			var competitions = await _applicationDbContext.Competitions.ToListAsync();

			return competitions;
		}

		private async Task SaveUploadedFileAsync(IFormFile file, string folderType, string propertyName, Competition competition, string fileName)
		{
			if (file == null || file.Length == 0)
				return;

			var folderPath = Path.Combine("wwwroot", folderType, "competitions");
			var fullPath = Path.Combine(folderPath, fileName);

			Directory.CreateDirectory(folderPath);

			using var stream = new FileStream(fullPath, FileMode.Create);
			await file.CopyToAsync(stream);

			var relativePath = $"/{folderType}/competitions/{fileName}";

			var property = typeof(Competition).GetProperty(propertyName);
			if (property != null && property.CanWrite)
			{
				property.SetValue(competition, relativePath);
				await _applicationDbContext.SaveChangesAsync();
			}
		}
	}
}