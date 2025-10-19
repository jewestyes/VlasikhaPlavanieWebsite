using Microsoft.AspNetCore.Mvc;
using VlasikhaPlavanieWebsite.Models;
using VlasikhaPlavanieWebsite.Application.Interfaces;

namespace VlasikhaPlavanieWebsite.Controllers
{
    [Route("tinkoff")]
    public class TinkoffController : Controller
    {
        private readonly ILogger<TinkoffController> _logger;
		private readonly ITinkoffWebhookService _webhookService;

		public TinkoffController(
			ILogger<TinkoffController> logger,
			ITinkoffWebhookService webhookService)
        {
            _logger = logger;
			_webhookService = webhookService;
		}
		[HttpPost("webhook")]
		public async Task<IActionResult> AlfaWebhook([FromBody] TinkoffWebhook model)
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