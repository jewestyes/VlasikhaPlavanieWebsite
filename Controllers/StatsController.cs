using Microsoft.AspNetCore.Mvc;
using VlasikhaPlavanieWebsite.Models;
using VlasikhaPlavanieWebsite.Data;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace VlasikhaPlavanieWebsite.Controllers
{
    public class StatsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public StatsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var stats = await _context.StatItems.ToListAsync();
            return View(stats);
        }

        [HttpPost]
        public async Task<IActionResult> AddStat(string date, string name, string city)
        {
            if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(city))
            {
                return BadRequest("Invalid input");
            }

            var newItem = new StatItem
            {
                Date = date,
                Name = name,
                City = city
            };

            _context.StatItems.Add(newItem);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStat(int id)
        {
            var stat = await _context.StatItems.FindAsync(id);
            if (stat == null)
            {
                return NotFound();
            }

            foreach (var file in stat.Files)
            {
                var filePath = Path.Combine(_env.WebRootPath, "Files", file);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.StatItems.Remove(stat);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int id)
        {
            var stat = await _context.StatItems.FindAsync(id);
            if (stat == null)
            {
                return NotFound();
            }
            return View(stat);
        }

        [HttpPost]
        public async Task<IActionResult> AddFile(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Invalid file");
            }

            var stat = await _context.StatItems.FindAsync(id);
            if (stat == null)
            {
                return NotFound();
            }

            var uploads = Path.Combine(_env.WebRootPath, "Files");
            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }

            var filePath = Path.Combine(uploads, file.FileName);

            if (System.IO.File.Exists(filePath))
            {
                ModelState.AddModelError("File", "A file with this name already exists.");
                return View("Details", stat);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            stat.Files.Add(file.FileName);
            _context.StatItems.Update(stat);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = id });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFile(int id, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("Invalid file name");
            }

            var stat = await _context.StatItems.FindAsync(id);
            if (stat == null)
            {
                return NotFound();
            }

            stat.Files.Remove(fileName);
            var filePath = Path.Combine(_env.WebRootPath, "Files", fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.StatItems.Update(stat);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = id });
        }
    }
}
