using VlasikhaPlavanieWebsite.Models;

namespace VlasikhaPlavanieWebsite.Application.Interfaces
{
	public interface IHomeService
	{
		Task<Dictionary<string, (string FilePath, bool IsExternalLink)>> GetButtonFilesAsync();
		Task<Competition> GetActiveStagesAsync();
	}
}