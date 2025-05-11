using Microsoft.AspNetCore.Http;
using VlasikhaPlavanieWebsite.Models;

namespace VlasikhaPlavanieWebsite.Application.Interfaces
{
	public interface IStatService
	{
		Task<List<StatItem>> GetAllAsync();
		Task<StatItem?> GetByIdAsync(int id);
		Task AddAsync(string date, string name, string city);
		Task DeleteAsync(int id);
		Task AddFileAsync(int id, IFormFile file);
		Task DeleteFileAsync(int id, string fileName);
	}
}
