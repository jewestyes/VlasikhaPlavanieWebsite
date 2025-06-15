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
			var openCompetitions = await _homeService.GetOpenCompetitionsAsync();
			return View(openCompetitions);
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
