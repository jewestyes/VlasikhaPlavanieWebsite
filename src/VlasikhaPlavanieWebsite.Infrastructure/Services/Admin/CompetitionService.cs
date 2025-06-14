using Microsoft.EntityFrameworkCore;
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
	}
}