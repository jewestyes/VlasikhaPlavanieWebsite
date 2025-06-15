using Microsoft.AspNetCore.Mvc;
using VlasikhaPlavanieWebsite.Application.Interfaces;

namespace VlasikhaPlavanieWebsite.Controllers
{
    public class PrivacyController : BaseController
    {
		public PrivacyController(IHomeService homeService) : base(homeService)
		{
		}

		public IActionResult Index()
        {
            return View();
        }
    }
}
