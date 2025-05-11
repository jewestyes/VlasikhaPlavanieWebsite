using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VlasikhaPlavanieWebsite.Application.Interfaces;
using VlasikhaPlavanieWebsite.ViewModels;

namespace VlasikhaPlavanieWebsite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeService _homeService;

        public HomeController(ILogger<HomeController> logger, IHomeService homeService)
        {
            _logger = logger;
            _homeService = homeService;
        }

		public async Task<IActionResult> Index()
		{
            try
            {
                var buttonFiles = await _homeService.GetButtonFilesAsync();

                var activeStage = await _homeService.GetActiveStagesAsync();

                ViewData["StageName"] = activeStage?.StageName ?? "Неизвестный этап";
                ViewData["CompetitionDate"] = activeStage?.CompetitionDate?.ToString("dd MMMM yyyy") ?? "Дата не указана";
                ViewData["CompetitionAddress"] = activeStage?.CompetitionAddress ?? "Адрес не указан";

                return View(buttonFiles);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[HomeController] [Index] ERROR: {ex.Message}");
            }
            return View(new Dictionary<string, string>());
        }

		public IActionResult Registration()
        {
            return View();
        }

        public IActionResult Stats()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
