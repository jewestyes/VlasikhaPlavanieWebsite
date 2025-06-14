using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VlasikhaPlavanieWebsite.Application.Interfaces;
using VlasikhaPlavanieWebsite.ViewModels;

namespace VlasikhaPlavanieWebsite.Controllers
{
	public class HomeController : BaseController
	{

		public HomeController(ILogger<HomeController> logger, IHomeService homeService)
			: base(homeService)
		{
		}

		public async Task<IActionResult> Index()
		{
			var activeStage = await _homeService.GetActiveStagesAsync();
			ViewData["ButtonFiles"] = await _homeService.GetButtonFilesAsync();
			ViewData["StageName"] = activeStage?.StageName ?? "Неизвестный этап";
			ViewData["CompetitionDate"] = activeStage?.CompetitionDate?.ToString("dd MMMM yyyy") ?? "Дата не указана";
			ViewData["CompetitionAddress"] = activeStage?.CompetitionAddress ?? "Адрес не указан";
			return View();
		}

		public IActionResult Registration() => View();

		public IActionResult Stats() => View();

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
