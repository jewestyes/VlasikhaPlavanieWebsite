using Microsoft.EntityFrameworkCore;
using VlasikhaPlavanieWebsite.Application.Interfaces;
using VlasikhaPlavanieWebsite.Data;
using VlasikhaPlavanieWebsite.Models;

namespace VlasikhaPlavanieWebsite.Infrastructure.Services
{
	public class HomeService : IHomeService
	{
        private readonly ApplicationDbContext _applicationDbContext;
        public HomeService(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

		public async Task<Dictionary<string, (string FilePath, bool IsExternalLink)>> GetButtonFilesAsync()
		{
			var fileMappings = await _applicationDbContext.FileMappings
				.Select(f => new
				{
					f.ButtonName,
					Path = f.FilePath ?? "#",
					f.IsExternalLink
				})
				.ToListAsync();

			return fileMappings.ToDictionary(
				x => x.ButtonName,
				x => (
					FilePath: x.IsExternalLink
						? x.Path
						: $"/Files/{Path.GetFileName(x.Path)}",
					IsExternalLink: x.IsExternalLink
				)
			);
		}

		public async Task<List<Competition>> GetOpenCompetitionsAsync()
		{
			return await _applicationDbContext.Competitions
					.Where(s => s.IsOpen)
					.OrderBy(s => s.CompetitionStartDate)
					.ToListAsync();
		}
	}
}
