using Microsoft.AspNetCore.Mvc;
using VlasikhaPlavanieWebsite.Application.Interfaces;

namespace VlasikhaPlavanieWebsite.Controllers
{
    public class StatsController : Controller
	{
		private readonly IStatService _statService;

		public StatsController(IStatService statService)
        {
			_statService = statService;
		}

        public async Task<IActionResult> Index()
        {
			var stats = await _statService.GetAllAsync();
			return View(stats);
		}

        [HttpPost]
        public async Task<IActionResult> AddStat(string date, string name, string city)
        {
			try
			{
				await _statService.AddAsync(date, name, city);
				return RedirectToAction("Index");
			}
			catch
			{
				return BadRequest("Invalid input");
			}
		}

        [HttpPost]
        public async Task<IActionResult> DeleteStat(int id)
        {
			try
			{
				await _statService.DeleteAsync(id);
				return RedirectToAction("Index");
			}
			catch
			{
				return NotFound();
			}
		}

        public async Task<IActionResult> Details(int id)
        {
			var stat = await _statService.GetByIdAsync(id);
			if (stat == null)
				return NotFound();

			return View(stat);
		}

        [HttpPost]
        public async Task<IActionResult> AddFile(int id, IFormFile file)
        {
			try
			{
				await _statService.AddFileAsync(id, file);
				return RedirectToAction("Details", new { id });
			}
			catch (IOException)
			{
				ModelState.AddModelError("File", "A file with this name already exists.");
				var stat = await _statService.GetByIdAsync(id);
				return View("Details", stat);
			}
			catch
			{
				return BadRequest("Invalid input");
			}
		}

        [HttpPost]
        public async Task<IActionResult> DeleteFile(int id, string fileName)
        {
			try
			{
				await _statService.DeleteFileAsync(id, fileName);
				return RedirectToAction("Details", new { id });
			}
			catch
			{
				return BadRequest("Invalid input");
			}
		}
    }
}