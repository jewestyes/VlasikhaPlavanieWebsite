using Microsoft.AspNetCore.Mvc;

namespace VlasikhaPlavanieWebsite.Controllers
{
    public class ContactsController : Controller
    {
        public ContactsController() {}

		public IActionResult Index()
		{
            return View();
        }
    }
}
