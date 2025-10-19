using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using VlasikhaPlavanieWebsite.Models;

namespace VlasikhaPlavanieWebsite.Controllers
{
	[Route("alfa")]
	public class AlfaController : Controller
	{
		private readonly ILogger<AlfaController> _logger;

		public AlfaController(ILogger<AlfaController> logger)
		{
			_logger = logger;
		}

		[HttpPost("webhook")]
		public IActionResult AlfaWebhook([FromBody] JsonElement payload)
		{
			_logger.LogInformation("Alfa webhook raw JSON: {Json}", payload.GetRawText());

			return Ok("OK");
		}
	}
}
