using Microsoft.AspNetCore.Mvc;

namespace VlasikhaPlavanieWebsite.Controllers
{
    public class StatsController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public StatsController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
