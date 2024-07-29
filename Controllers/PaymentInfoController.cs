using Microsoft.AspNetCore.Mvc;

namespace VlasikhaPlavanieWebsite.Controllers
{

    public class PaymentInfoController : Controller
    {
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
