using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Serilog;
using System.Text.Json;
using VlasikhaPlavanieWebsite.Data;
using VlasikhaPlavanieWebsite.Infrastructure.Services.Admin;
using VlasikhaPlavanieWebsite.Models;
using VlasikhaPlavanieWebsite.ViewModels;

namespace VlasikhaPlavanieWebsite.Controllers
{

	[Authorize(Roles = "Admin")]
	public class AdminController : Controller
	{
		private readonly IWebHostEnvironment _webHostEnvironment;
		private readonly ILogger<AdminController> _logger;
		private readonly IAdminAuthService _adminAuthService;
		private readonly IParticipantExportService _participantExportService;
		private readonly ApplicationDbContext _context;

		public AdminController(IWebHostEnvironment webHostEnvironment, ILogger<AdminController> logger,
							IAdminAuthService adminAuthService, IParticipantExportService participantExportService, ApplicationDbContext context)
		{
			_logger = logger;
			_webHostEnvironment = webHostEnvironment;
			_context = context;
			_adminAuthService = adminAuthService;
			_participantExportService = participantExportService;
		}

		[HttpGet]
		[AllowAnonymous]
		[Route("Admin/Login")]
		public IActionResult Login()
		{
			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("Admin/Login")]
		public async Task<IActionResult> Login(LoginViewModel model)
		{
			if (!ModelState.IsValid)
			{
				Log.Warning("Invalid model state for login attempt. Email: {Email}, ModelState: {ModelStateErrors}",
							model.Email,
							string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
				return View(model);
			}

			var (succeeded, errors) = await _adminAuthService.LoginAsync(model, HttpContext);

			if (!succeeded)
			{
				foreach (var error in errors)
					ModelState.AddModelError(string.Empty, error);

				return View(model);
			}

			return RedirectToAction("Index", "Admin");
		}


		[HttpGet]
		[Route("Admin/Logout")]
		public async Task<IActionResult> Logout()
		{
			var userName = User.Identity?.Name ?? "Unknown user";
			Log.Information("User {UserName} is attempting to log out.", userName);

			try
			{
				await _adminAuthService.LogoutAsync(HttpContext);

				Log.Information("User {UserName} has successfully logged out.", userName);
				Log.Information("All cookies deleted for user {UserName}.", userName);
			}
			catch (Exception ex)
			{
				Log.Error("An error occurred while attempting to logout the user {userName}:\n{ex.Message}", userName, ex.Message);
				throw new Exception($"An error occurred while attempting to logout the user {userName}:\n{ex.Message}");
			}

			return RedirectToAction("Index", "Home");
		}

		[HttpGet]
		[Route("Admin/Index")]
		public async Task<IActionResult> Index()
		{
			var query = from p in _context.Participants
						join o in _context.Orders on p.OrderId equals o.Id into po
						from order in po.DefaultIfEmpty()
						join d in _context.Disciplines on p.Id equals d.ParticipantId into pd
						from discipline in pd.DefaultIfEmpty()
						join rs in _context.RegistrationStage on order.RegistrationStageId equals rs.Id into ors
						from regStage in ors.DefaultIfEmpty()
						select new ParticipantOrderViewModel
						{
							LastName = p.LastName,
							FirstName = p.FirstName,
							MiddleName = p.MiddleName,
							BirthDate = p.BirthDate,
							Gender = p.Gender,
							CityOrTeam = p.CityOrTeam,
							Rank = p.Rank,
							Phone = p.Phone,
							CreatedAt = order.CreatedAt,
							Email = p.Email,
							DisciplineName = discipline != null ? discipline.Name : null,
							Distance = discipline != null ? discipline.Distance : null,
							EntryTime = discipline != null ? discipline.EntryTime : null,
							OrderNumber = order != null ? order.OrderNumber : null,
							Amount = order != null ? order.Amount : 0m,
							RegistrationStageName = regStage != null ? regStage.StageName : "Неизвестный этап"
						};

			var result = await query.ToListAsync();
			return View(result);
		}

		[HttpGet]
		[Route("Admin/DownloadParticipantsExcelByStage")]
		public async Task<IActionResult> DownloadParticipantsExcelByStage(string stageName)
		{
			if (string.IsNullOrEmpty(stageName))
			{
				return BadRequest("Не указан этап регистрации.");
			}

			var excelBytes = await _participantExportService.ExportByStageAsync(stageName);

			var fileName = $"Participants_{stageName}_{DateTime.UtcNow.ToString("yyyyMMdd_HHmmss")}.xlsx";
			var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

			return File(excelBytes, contentType, fileName);
		}

		[HttpGet]
		[Route("Admin/EditFiles")]
		public async Task<IActionResult> EditFiles()
		{
			try
			{
				var fileMappings = await _context.FileMappings
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
							buttonFiles[key] = (Url.Content($"~/Files/{fileName}"), false);
						}
						else
						{
							buttonFiles[key] = ("#", false);
						}
					}
				}

				return View(buttonFiles);
			}
			catch (Exception ex)
			{
				_logger.LogError($"[HomeController] [Index] ERROR: {ex.Message}");
				return View(new Dictionary<string, (string, bool)>());
			}
		}

		[HttpPost]
		[Route("Admin/EditFiles")]
		public async Task<IActionResult> EditFiles(string buttonName, IFormFile newFile, string externalLink)
		{
			var fileMapping = await _context.FileMappings.FirstOrDefaultAsync(f => f.ButtonName == buttonName);

			if (!string.IsNullOrWhiteSpace(externalLink))
			{
				if (fileMapping == null)
				{
					fileMapping = new FileMapping
					{
						ButtonName = buttonName,
						FileName = "External Link",
						FilePath = externalLink,
						IsExternalLink = true
					};
					_context.FileMappings.Add(fileMapping);
				}
				else
				{
					fileMapping.FileName = "External Link";
					fileMapping.FilePath = externalLink;
					fileMapping.IsExternalLink = true;
					_context.FileMappings.Update(fileMapping);
				}
			}
			else if (newFile != null && newFile.Length > 0)
			{
				var extension = Path.GetExtension(newFile.FileName);
				var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".xls", ".txt" };

				if (!allowedExtensions.Contains(extension.ToLower()))
				{
					ModelState.AddModelError("", "Недопустимый формат файла.");
					return RedirectToAction("EditFiles");
				}

				var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "Files");

				if (!Directory.Exists(uploadPath))
				{
					Directory.CreateDirectory(uploadPath);
				}

				var fileName = $"{buttonName}_{DateTime.Now.Ticks}{extension}";
				var filePath = Path.Combine("Files", fileName);

				if (fileMapping != null && !string.IsNullOrEmpty(fileMapping.FilePath) && !fileMapping.IsExternalLink)
				{
					var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, fileMapping.FilePath);
					if (System.IO.File.Exists(oldFilePath))
					{
						System.IO.File.Delete(oldFilePath);
					}
				}

				using (var stream = new FileStream(Path.Combine(_webHostEnvironment.WebRootPath, filePath), FileMode.Create))
				{
					await newFile.CopyToAsync(stream);
				}

				if (fileMapping == null)
				{
					fileMapping = new FileMapping
					{
						ButtonName = buttonName,
						FileName = newFile.FileName,
						FilePath = filePath,
						IsExternalLink = false
					};
					_context.FileMappings.Add(fileMapping);
				}
				else
				{
					fileMapping.FileName = newFile.FileName;
					fileMapping.FilePath = filePath;
					fileMapping.IsExternalLink = false;
					_context.FileMappings.Update(fileMapping);
				}
			}
			else
			{
				if (fileMapping == null)
				{
					fileMapping = new FileMapping
					{
						ButtonName = buttonName,
						FileName = "N/A",
						FilePath = "#",
						IsExternalLink = false
					};
					_context.FileMappings.Add(fileMapping);
				}
			}

			await _context.SaveChangesAsync();
			return RedirectToAction("EditFiles");
		}

		[HttpPost]
		[Route("Admin/DeleteFile")]
		public async Task<IActionResult> DeleteFile(string buttonName)
		{
			if (string.IsNullOrEmpty(buttonName))
			{
				return BadRequest("ButtonName не может быть пустым.");
			}

			var fileMapping = await _context.FileMappings.FirstOrDefaultAsync(f => f.ButtonName == buttonName);

			if (fileMapping != null)
			{
				if (!string.IsNullOrEmpty(fileMapping.FilePath) && System.IO.File.Exists(fileMapping.FilePath))
				{
					System.IO.File.Delete(fileMapping.FilePath);
				}

				fileMapping.FilePath = "#";
				fileMapping.FileName = "#";
				_context.FileMappings.Update(fileMapping);

				await _context.SaveChangesAsync();
			}

			return RedirectToAction("EditFiles");
		}

		[HttpGet]
		[Route("Admin/ManageStages")]
		public async Task<IActionResult> ManageStages()
		{
			var stages = await _context.RegistrationStage.ToListAsync();
			var model = new ManageStagesViewModel
			{
				Stages = stages,
				NewStage = new RegistrationStage()
			};
			return View(model);
		}

		[HttpPost]
		[Route("Admin/CreateStage")]
		public async Task<IActionResult> CreateStage(ManageStagesViewModel model)
		{
			ModelState.Remove("Stages");

			if (ModelState.IsValid)
			{
				model.NewStage.IsOpen = false;
				_context.RegistrationStage.Add(model.NewStage);
				await _context.SaveChangesAsync();

				if (model.SelectedDisciplines != null && model.SelectedDisciplines.Any())
				{
					var stageDisciplines = new List<StageDiscipline>();

					foreach (var discipline in model.SelectedDisciplines)
					{
						if (model.DisciplineDistances.TryGetValue(discipline, out string distances) && !string.IsNullOrWhiteSpace(distances))
						{
							var distanceList = distances.Split(';')
								.Select(d => d.Trim())
								.Where(d => !string.IsNullOrEmpty(d))
								.ToList();

							stageDisciplines.Add(new StageDiscipline
							{
								StageId = model.NewStage.Id,
								Name = discipline,
								DistancesJson = JsonSerializer.Serialize(distanceList)
							});
						}
					}

					if (stageDisciplines.Any())
					{
						await _context.StageDisciplines.AddRangeAsync(stageDisciplines);
						await _context.SaveChangesAsync();
					}
				}

				return RedirectToAction("ManageStages");
			}

			// Если валидация не прошла, загружаем существующие этапы и возвращаем форму
			model.Stages = await _context.RegistrationStage.ToListAsync();
			return View("ManageStages", model);
		}

		[HttpPost]
		[Route("Admin/ChangeStageStatus")]
		public async Task<IActionResult> ChangeStageStatus(int id, bool isOpen)
		{
			var stage = await _context.RegistrationStage.FindAsync(id);
			if (stage != null)
			{
				stage.IsOpen = isOpen;
				if (!isOpen)
				{
					stage.RegistrationEndDate = DateTime.UtcNow.AddHours(3);
				}
				else
				{
					stage.RegistrationEndDate = null;
				}
				await _context.SaveChangesAsync();
			}
			return RedirectToAction("ManageStages");
		}
	}
}
