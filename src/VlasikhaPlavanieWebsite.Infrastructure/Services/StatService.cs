using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using VlasikhaPlavanieWebsite.Application.Interfaces;
using VlasikhaPlavanieWebsite.Data;
using VlasikhaPlavanieWebsite.Models;

namespace VlasikhaPlavanieWebsite.Infrastructure.Services
{
	public class StatService : IStatService
	{
		private readonly ApplicationDbContext _applicationDbContext;
		private readonly IWebHostEnvironment _webHostEnvironment;
		public StatService(ApplicationDbContext applicationDbContext, IWebHostEnvironment webHostEnvironment)
		{
			_applicationDbContext = applicationDbContext;
			_webHostEnvironment = webHostEnvironment;
		}
		public async Task AddAsync(string date, string name, string city)
		{
			if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(city))
				throw new ArgumentException("Invalid input");

			var newItem = new StatItem { Date = date, Name = name, City = city };
			_applicationDbContext.StatItems.Add(newItem);
			await _applicationDbContext.SaveChangesAsync();
		}

		public async Task AddFileAsync(int id, IFormFile file)
		{
			if (file == null || file.Length == 0)
				throw new ArgumentException("Invalid file");

			var stat = await _applicationDbContext.StatItems.FindAsync(id);
			if (stat == null)
				throw new KeyNotFoundException("Stat not found");

			var uploads = Path.Combine(_webHostEnvironment.WebRootPath, "Files");
			if (!Directory.Exists(uploads))
				Directory.CreateDirectory(uploads);

			var filePath = Path.Combine(uploads, file.FileName);
			if (System.IO.File.Exists(filePath))
				throw new IOException("File already exists");

			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await file.CopyToAsync(stream);
			}

			stat.Files.Add(file.FileName);
			_applicationDbContext.StatItems.Update(stat);
			await _applicationDbContext.SaveChangesAsync();
		}

		public async Task DeleteAsync(int id)
		{
			var stat = await _applicationDbContext.StatItems.FindAsync(id);
			if (stat == null)
				throw new KeyNotFoundException("Stat not found");

			foreach (var file in stat.Files)
			{
				var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Files", file);
				if (System.IO.File.Exists(filePath))
					System.IO.File.Delete(filePath);
			}

			_applicationDbContext.StatItems.Remove(stat);
			await _applicationDbContext.SaveChangesAsync();
		}

		public async Task DeleteFileAsync(int id, string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
				throw new ArgumentException("Invalid file name");

			var stat = await _applicationDbContext.StatItems.FindAsync(id);
			if (stat == null)
				throw new KeyNotFoundException("Stat not found");

			stat.Files.Remove(fileName);

			var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Files", fileName);
			if (System.IO.File.Exists(filePath))
				System.IO.File.Delete(filePath);

			_applicationDbContext.StatItems.Update(stat);
			await _applicationDbContext.SaveChangesAsync();
		}

		public async Task<List<StatItem>> GetAllAsync() =>
			await _applicationDbContext.StatItems.ToListAsync();

		public async Task<StatItem?> GetByIdAsync(int id) =>
			await _applicationDbContext.StatItems.FindAsync(id);
	}
}
