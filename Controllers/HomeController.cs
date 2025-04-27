using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using VlasikhaPlavanieWebsite.Data;
using VlasikhaPlavanieWebsite.Models;
using VlasikhaPlavanieWebsite.ViewModels;

namespace VlasikhaPlavanieWebsite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

		public async Task<IActionResult> Index()
		{
            try
            {
                var fileMappings = await _context.FileMappings
                    .Select(f => new
                    {
                        f.ButtonName,
                        FilePath = f.FilePath ?? "#",
						f.IsExternalLink
					})
                    .ToListAsync();

                var buttonFiles = fileMappings.ToDictionary(
                    f => f.ButtonName,
					f => (FilePath: f.IsExternalLink ? f.FilePath : Url.Content($"~/Files/{Path.GetFileName(f.FilePath)}"),
				  IsExternalLink: f.IsExternalLink));


                var activeStage = await _context.RegistrationStage
                    .Where(s => s.IsOpen)
                    .OrderByDescending(s => s.RegistrationStartDate)
                    .FirstOrDefaultAsync();

                ViewData["StageName"] = activeStage?.StageName ?? "Неизвестный этап";
                ViewData["CompetitionDate"] = activeStage?.CompetitionDate?.ToString("dd MMMM yyyy") ?? "Дата не указана";
                ViewData["CompetitionAddress"] = activeStage?.CompetitionAddress ?? "Адрес не указан";

                return View(buttonFiles);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[HomeController] [Index] ERROR: {ex.Message}");
            }
            return View(new Dictionary<string,string>());
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
