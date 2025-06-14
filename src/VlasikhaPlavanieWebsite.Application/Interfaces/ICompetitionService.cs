using VlasikhaPlavanieWebsite.Models;
using VlasikhaPlavanieWebsite.ViewModels;

namespace VlasikhaPlavanieWebsite.Infrastructure.Services.Admin
{
	public interface ICompetitionService
	{
		Task CreateCompetitionAsync(ManageCompetitionsViewModel model);
		Task ChangeStatusAsync(int id, bool isOpen);
		Task<List<Competition>> GetAllAsync();
	}
}