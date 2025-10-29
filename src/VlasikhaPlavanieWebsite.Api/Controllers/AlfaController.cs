using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlasikhaPlavanieWebsite.Application.Interfaces;
using VlasikhaPlavanieWebsite.Models;

namespace VlasikhaPlavanieWebsite.Controllers
{
	[Route("alfa")]
	public class AlfaController : Controller
	{
		private readonly ILogger<AlfaController> _logger;
		private readonly IAlfaWebhookService _alfaWebhookService;

		public AlfaController(ILogger<AlfaController> logger, IAlfaWebhookService alfaWebhookService)
		{
			_logger = logger;
			_alfaWebhookService = alfaWebhookService;
		}


		[HttpGet("webhook")]
		[AllowAnonymous]
		[IgnoreAntiforgeryToken]
		public IActionResult AlfaWebhook([FromQuery] AlfaCallbackQuery model)
		{
			//Console.WriteLine($"{model.orderNumber}\n{model.checksum}\n{model.mdOrder}\n{model.operation}\n{model.status}");

			var result = _alfaWebhookService.HandleWebhookAsync(model, Request.Query).GetAwaiter().GetResult();

			return result;
		}

	}
}