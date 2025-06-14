using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VlasikhaPlavanieWebsite.Data;
using VlasikhaPlavanieWebsite.Models;
using VlasikhaPlavanieWebsite.ViewModels;

namespace VlasikhaPlavanieWebsite.Infrastructure.Services.Admin
{
	public class StageService : IStageService
	{
		private readonly ApplicationDbContext _applicationDbContext;
		public StageService(ApplicationDbContext applicationDbContext)
		{
			_applicationDbContext = applicationDbContext;
		}
		public async Task ChangeStatusAsync(int id, bool isOpen)
		{
			var stage = await _applicationDbContext.RegistrationStage.FindAsync(id);

			if (stage != null)
			{
				stage.IsOpen = isOpen;

				if (!isOpen)
				{
					stage.RegistrationEndDate = DateTime.UtcNow.AddHours(3);
				}
				else
				{
					stage.RegistrationEndDate = null;
				}
				await _applicationDbContext.SaveChangesAsync();
			}
		}

		public async Task CreateStageAsync(ManageStagesViewModel model)
		{

			model.NewStage.IsOpen = false;
			_applicationDbContext.RegistrationStage.Add(model.NewStage);
			await _applicationDbContext.SaveChangesAsync();

			if (model.SelectedDisciplines != null && model.SelectedDisciplines.Any())
			{
				var stageDisciplines = new List<CompetitionDiscipline>();

				foreach (var discipline in model.SelectedDisciplines)
				{
					if (model.DisciplineDistances.TryGetValue(discipline, out string distances)
						&& !string.IsNullOrWhiteSpace(distances))
					{
						var distanceList = distances.Split(';')
							.Select(d => d.Trim())
							.Where(d => !string.IsNullOrEmpty(d))
							.ToList();

						stageDisciplines.Add(new CompetitionDiscipline
						{
							CompetitionId = model.NewStage.Id,
							Name = discipline,
							DistancesJson = JsonSerializer.Serialize(distanceList)
						});
					}
				}

				if (stageDisciplines.Any())
				{
					await _applicationDbContext.StageDisciplines.AddRangeAsync(stageDisciplines);
					await _applicationDbContext.SaveChangesAsync();
				}
			}
		}

		public async Task<List<Competition>> GetAllAsync()
		{
			var stages = await _applicationDbContext.RegistrationStage.ToListAsync();

			return stages;
		}
	}
}