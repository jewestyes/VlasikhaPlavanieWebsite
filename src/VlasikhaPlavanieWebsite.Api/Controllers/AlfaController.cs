using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
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


		[HttpGet("webhook")]
		[AllowAnonymous]
		[IgnoreAntiforgeryToken]
		public IActionResult AlfaWebhook(
			[FromQuery] string mdOrder,
			[FromQuery] string orderNumber,
			[FromQuery] string checksum,
			[FromQuery] string callbackCreationDate,
			[FromQuery] string operation,
			[FromQuery] string status,
			[FromQuery] string bindingId,
			[FromQuery] string clientId,
			[FromQuery] string enabled,
			[FromQuery] string operationRefundedAmount,
			[FromQuery] string operationRefundedAmountFormatted
)
		{
			_logger.LogInformation(
				"Alfa GET webhook: mdOrder={mdOrder}, orderNumber={orderNumber}," +
				" checksum={checksum}, callbackCreationDate={callbackCreationDate}," +
				" operation={operation}, status={status}, bindingId={bindingId}," +
				" clientId={clientId}, enabled={enabled}, operationRefundedAmount={operationRefundedAmount}," +
				" operationRefundedAmountFormatted={operationRefundedAmountFormatted}",
				mdOrder,
				orderNumber,
				checksum,
				callbackCreationDate,
				operation,
				status,
				bindingId,
				clientId,
				enabled,
				operationRefundedAmount,
				operationRefundedAmountFormatted
			);

			return Ok("OK");
		}

	}
}
