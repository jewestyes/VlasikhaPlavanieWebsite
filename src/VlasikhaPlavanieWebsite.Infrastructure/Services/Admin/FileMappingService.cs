using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using VlasikhaPlavanieWebsite.Data;
using VlasikhaPlavanieWebsite.Infrastructure.Exceptions.Admin;
using VlasikhaPlavanieWebsite.Models;

namespace VlasikhaPlavanieWebsite.Infrastructure.Services.Admin
{
	public class FileMappingService : IFileMappingService
	{
		private readonly ApplicationDbContext _applicationDbContext;
		private readonly IWebHostEnvironment _webHostEnvironment;
		private static readonly string FilesFolder = "Files";
		private static readonly string[] AllowedExtensions = { ".pdf", ".docx", ".xlsx", ".xls", ".txt" };
		public FileMappingService(ApplicationDbContext applicationDbContext, IWebHostEnvironment webHostEnvironment)
		{
			_applicationDbContext = applicationDbContext;
			_webHostEnvironment = webHostEnvironment;
		}

		public async Task DeleteFileAsync(string buttonName)
		{
			var fileMapping = await _applicationDbContext.FileMappings.FirstOrDefaultAsync(f => f.ButtonName == buttonName);

			if (fileMapping != null)
			{
				if (!string.IsNullOrEmpty(fileMapping.FilePath) && System.IO.File.Exists(fileMapping.FilePath))
				{
					System.IO.File.Delete(fileMapping.FilePath);
				}

				fileMapping.FilePath = "#";
				fileMapping.FileName = "#";
				_applicationDbContext.FileMappings.Update(fileMapping);

				await _applicationDbContext.SaveChangesAsync();
			}
		}

		public async Task<Dictionary<string, (string FilePath, bool IsExternalLink)>> GetAllMappingsAsync()
		{
			var fileMappings = await _applicationDbContext.FileMappings
							.Select(f => new
							{
								f.ButtonName,
								FileName = f.FileName ?? "#",
								FilePath = f.FilePath ?? "#",
								f.IsExternalLink
							})
							.ToListAsync();

			var buttonFiles = fileMappings.ToDictionary(
				f => f.ButtonName,
				f => (FilePath: f.FilePath, IsExternalLink: f.IsExternalLink)
			);

			foreach (var key in buttonFiles.Keys.ToList())
			{
				var relativePath = buttonFiles[key].FilePath;

				if (!buttonFiles[key].IsExternalLink)
				{
					var fileName = Path.GetFileName(relativePath);
					var fileDirectory = Path.Combine("Files", fileName);
					var absolutePath = Path.Combine(_webHostEnvironment.WebRootPath, "Files", fileName);

					if (System.IO.File.Exists(absolutePath))
					{
						buttonFiles[key] = ("/Files/" + fileName, false);
					}
					else
					{
						buttonFiles[key] = ("#", false);
					}
				}
			}

			return buttonFiles;
		}

		public async Task SaveMappingAsync(string buttonName, IFormFile file, string externalLink)
		{
			if (!string.IsNullOrWhiteSpace(externalLink))
			{
				await UpsertMappingAsync(buttonName, "External Link", externalLink, true);

				return;
			}

			if (file != null && file.Length > 0)
			{
				ValidateExtension(file.FileName);

				var newPath = await SaveFileAsync(buttonName, file);
				await UpsertMappingAsync(buttonName, file.FileName, newPath, false);

				return;
			}

			await EnsureMappingExistsAsync(buttonName);
		}

		private void ValidateExtension(string fileName)
		{
			var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
			if (!AllowedExtensions.Contains(extension))
			{
				throw new InvalidFileExtensionException(fileName);
			}
		}

		private async Task<string> SaveFileAsync(string buttonName, IFormFile file)
		{
			var uploads = Path.Combine(_webHostEnvironment.WebRootPath, FilesFolder);
			Directory.CreateDirectory(uploads);

			var fileName = $"{buttonName}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{Path.GetExtension(file.FileName)}";
			var fullPath = Path.Combine(uploads, fileName);

			var existing = await _applicationDbContext.FileMappings.FirstOrDefaultAsync(f => f.ButtonName == buttonName && !f.IsExternalLink);
			if (existing != null && !string.IsNullOrEmpty(existing.FilePath))
			{
				var old = Path.Combine(_webHostEnvironment.WebRootPath, existing.FilePath);
				if (File.Exists(old)) File.Delete(old);
			}

			using var stream = new FileStream(fullPath, FileMode.Create);
			await file.CopyToAsync(stream);
			return Path.Combine(FilesFolder, fileName);
		}

		private async Task UpsertMappingAsync(string buttonName, string storedName, string pathOrUrl, bool isExternal)
		{
			var map = await _applicationDbContext.FileMappings.FirstOrDefaultAsync(f => f.ButtonName == buttonName);
			if (map == null)
			{
				map = new FileMapping { ButtonName = buttonName };
				_applicationDbContext.FileMappings.Add(map);
			}
			map.FileName = storedName;
			map.FilePath = pathOrUrl;
			map.IsExternalLink = isExternal;
			await _applicationDbContext.SaveChangesAsync();
		}

		private async Task EnsureMappingExistsAsync(string buttonName)
		{
			var exists = await _applicationDbContext.FileMappings.AnyAsync(f => f.ButtonName == buttonName);
			if (!exists)
			{
				_applicationDbContext.FileMappings.Add(new FileMapping
				{
					ButtonName = buttonName,
					FileName = "N/A",
					FilePath = "#",
					IsExternalLink = false
				});
				await _applicationDbContext.SaveChangesAsync();
			}
		}
	}
}