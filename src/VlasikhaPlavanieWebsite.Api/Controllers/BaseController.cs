using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using VlasikhaPlavanieWebsite.Application.Interfaces;

namespace VlasikhaPlavanieWebsite.Controllers
{
	public class BaseController : Controller
	{
		protected readonly IHomeService _homeService;

		public BaseController(IHomeService homeService)
		{
			_homeService = homeService;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			var buttons = _homeService.GetButtonFilesAsync().GetAwaiter().GetResult();
			ViewData["Buttons"] = buttons;
			base.OnActionExecuting(context);
		}
	}
}
