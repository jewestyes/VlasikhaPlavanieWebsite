using Microsoft.AspNetCore.Mvc;

namespace VlasikhaPlavanieWebsite.Controllers
{
    public class PhotosController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public PhotosController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
