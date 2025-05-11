using VlasikhaPlavanieWebsite.ViewModels;

namespace VlasikhaPlavanieWebsite.Application.Interfaces
{
	public interface IRegistrationCache
	{
		Task<string> CacheAsync(RegistrationViewModel model);
	}
}