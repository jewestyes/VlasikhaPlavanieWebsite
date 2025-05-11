using Microsoft.AspNetCore.Http;

namespace VlasikhaPlavanieWebsite.Infrastructure.Services.Admin
{
	public interface IFileMappingService
	{
		Task<Dictionary<string, (string FilePath, bool IsExternalLink)>> GetAllMappingsAsync();
		Task SaveMappingAsync(string buttonName, IFormFile file, string webRootPath);
		Task DeleteFileAsync(string buttonName);
	}
}