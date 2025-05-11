using VlasikhaPlavanieWebsite.Models;
using VlasikhaPlavanieWebsite.ViewModels;

namespace VlasikhaPlavanieWebsite.Infrastructure.Services.Admin
{
	public interface IStageService
	{
		Task CreateStageAsync(ManageStagesViewModel model);
		Task ChangeStatusAsync(int id, bool isOpen);

		Task<List<RegistrationStage>> GetAllAsync();
	}
}