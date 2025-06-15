using Microsoft.AspNetCore.Mvc;
using VlasikhaPlavanieWebsite.Application.Interfaces;
using VlasikhaPlavanieWebsite.Infrastructure.Services;

namespace VlasikhaPlavanieWebsite.Controllers
{
	public class PaymentInfoController : BaseController
	{
		public PaymentInfoController(IHomeService homeService)
			: base(homeService)
		{

		}

        [HttpGet]
        public IActionResult PrivacyPolicy()
        {
            return View();
        }

        [HttpGet]
		public IActionResult TermsOfUse()
		{
			return View();
		}

		[HttpGet]
		public IActionResult Information()
		{
			return View();
		}

		[HttpGet]
		public IActionResult RefundPolicy()
		{
			return View();
		}

		[HttpGet]
		public IActionResult PaymentPolicy()
		{
			return View();
		}
	}
}