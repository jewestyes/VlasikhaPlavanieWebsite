using Microsoft.AspNetCore.Mvc;
using VlasikhaPlavanieWebsite.Application.Interfaces;
using VlasikhaPlavanieWebsite.Infrastructure.Services;

namespace VlasikhaPlavanieWebsite.Controllers
{
    public class ContactsController : BaseController
	{
        public ContactsController(IHomeService homeService)
            : base(homeService) { }

		public IActionResult Index()
		{
            return View();
        }
    }
}
