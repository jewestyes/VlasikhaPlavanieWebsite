using VlasikhaPlavanieWebsite.ViewModels;

namespace VlasikhaPlavanieWebsite.Application.Interfaces
{
	public interface IParticipantService
	{
		Task<List<ParticipantOrderViewModel>> GetAllParticipantOrdersAsync();
	}
}