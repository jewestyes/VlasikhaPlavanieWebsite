using Microsoft.AspNetCore.Mvc;
using VlasikhaPlavanieWebsite.Models;
using VlasikhaPlavanieWebsite.Application.Interfaces;

namespace VlasikhaPlavanieWebsite.Controllers
{
    [Route("alfa")]
    public class AlfaController : Controller
    {
        private readonly ILogger<AlfaController> _logger;
		private readonly ITinkoffWebhookService _webhookService;

		public AlfaController(
			ILogger<AlfaController> logger,
			ITinkoffWebhookService webhookService)
        {
            _logger = logger;
			_webhookService = webhookService;
		}
		[HttpPost("webhook")]
		public async Task<IActionResult> TinkoffWebhook([FromBody] TinkoffWebhook model)
		{
			try
			{
				return await _webhookService.HandleWebhookAsync(model);
			}
			catch (Exception ex)
			{
				_logger.LogCritical(ex, "Critical error in webhook for OrderId: {OrderId}", model.OrderId);
				return StatusCode(500, "Критическая ошибка обработки webhook.");
			}
		}
	}
}