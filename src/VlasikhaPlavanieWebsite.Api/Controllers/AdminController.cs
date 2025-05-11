using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using VlasikhaPlavanieWebsite.Application.Interfaces;
using VlasikhaPlavanieWebsite.Infrastructure.Exceptions.Admin;
using VlasikhaPlavanieWebsite.Infrastructure.Services.Admin;
using VlasikhaPlavanieWebsite.Models;
using VlasikhaPlavanieWebsite.ViewModels;

namespace VlasikhaPlavanieWebsite.Controllers
{

	[Authorize(Roles = "Admin")]
	public class AdminController : Controller
	{
		private readonly ILogger<AdminController> _logger;
		private readonly IAdminAuthService _adminAuthService;
		private readonly IParticipantExportService _participantExportService;
		private readonly IParticipantService _participantService;
		private readonly IStageService _stageService;
		private readonly IFileMappingService _fileMappingService;

		public AdminController(IWebHostEnvironment webHostEnvironment,
							   ILogger<AdminController> logger,
							   IAdminAuthService adminAuthService,
							   IParticipantExportService participantExportService,
							   IParticipantService participantService,
							   IStageService stageService,
							   IFileMappingService fileMappingService)
		{
			_logger = logger;
			_adminAuthService = adminAuthService;
			_participantExportService = participantExportService;
			_participantService = participantService;
			_stageService = stageService;
			_fileMappingService = fileMappingService;
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

			try
			{
				await _adminAuthService.LoginAsync(model, HttpContext);
				return RedirectToAction("Index", "Admin");
			}
			catch (UserNotFoundException ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
			}
			catch (UserNotInRoleException ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
			}
			catch (InvalidPasswordException ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Unhandled error during login.");
				ModelState.AddModelError(string.Empty, "Произошла непредвиденная ошибка.");
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
			return View(await _participantService.GetAllParticipantOrdersAsync());
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
				var buttonFiles = await _fileMappingService.GetAllMappingsAsync();

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
			try
			{
				await _fileMappingService.SaveMappingAsync(buttonName, newFile, externalLink);
				return RedirectToAction("EditFiles");
			}
			catch (InvalidFileExtensionException ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, "Ошибка при сохранении файла.");
				_logger.LogError(ex, "Unexpected error during file mapping.");
			}

			var mappings = await _fileMappingService.GetAllMappingsAsync();
			return View(mappings);
		}

		[HttpPost]
		[Route("Admin/DeleteFile")]
		public async Task<IActionResult> DeleteFile(string buttonName)
		{
			if (string.IsNullOrEmpty(buttonName))
			{
				return BadRequest("ButtonName не может быть пустым.");
			}

			await _fileMappingService.DeleteFileAsync(buttonName);

			return RedirectToAction("EditFiles");
		}

		[HttpGet]
		[Route("Admin/ManageStages")]
		public async Task<IActionResult> ManageStages()
		{
			var stages = await _stageService.GetAllAsync();
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
				await _stageService.CreateStageAsync(model);

				return RedirectToAction("ManageStages");
			}

			// Если валидация не прошла, загружаем существующие этапы и возвращаем форму
			model.Stages = await _stageService.GetAllAsync();
			return View("ManageStages", model);
		}

		[HttpPost]
		[Route("Admin/ChangeStageStatus")]
		public async Task<IActionResult> ChangeStageStatus(int id, bool isOpen)
		{
			await _stageService.ChangeStatusAsync(id, isOpen);

			return RedirectToAction("ManageStages");
		}
	}
}
