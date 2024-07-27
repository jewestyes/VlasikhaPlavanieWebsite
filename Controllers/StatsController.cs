using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlasikhaPlavanieWebsite.Models;
using System.Collections.Generic;
using System.Linq;

namespace VlasikhaPlavanieWebsite.Controllers
{
    public class StatsController : Controller
    {
        private static List<StatItem> _stats = new List<StatItem>
        {
            new StatItem { Id = 1, Date = "22-23 ДЕКАБРЯ 2030", Name = "МЕЖДУНАРОДНЫЕ СОРЕВНОВАНИЯ ПО ПЛАВАНИЮ \"КУБОК ГЛАВЫ ВЛАСИХИ\" - МОСКВА", City = "МОСКВА", Files = new List<string> { "DopInfo.pdf", "Polozhenie_MSK.pdf", "Кубок главы - итоговые.PDF" } },
            new StatItem { Id = 2, Date = "25-26 ДЕКАБРЯ 2030", Name = "МЕЖДУНАРОДНЫЕ СОРЕВНОВАНИЯ ПО ПЛАВАНИЮ \"КУБОК ГЛАВЫ ВЛАСИХИ\" - МОСКВА", City = "МОСКВА", Files = new List<string>() }
        };

        public IActionResult Index()
        {
            return View(_stats);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AddStat(string date, string name, string city)
        {
            var newItem = new StatItem
            {
                Id = _stats.Count + 1,
                Date = date,
                Name = name,
                City = city,
                Files = new List<string>()
            };
            _stats.Add(newItem);
            return RedirectToAction("Index");
        }

        public IActionResult Details(int id)
        {
            var stat = _stats.FirstOrDefault(s => s.Id == id);
            if (stat == null)
            {
                return NotFound();
            }
            return View(stat);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AddFile(int id, string fileName)
        {
            var stat = _stats.FirstOrDefault(s => s.Id == id);
            if (stat != null)
            {
                stat.Files.Add(fileName);
            }
            return RedirectToAction("Details", new { id = id });
        }
    }
}
