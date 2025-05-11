using Microsoft.AspNetCore.Mvc;
using VlasikhaPlavanieWebsite.Models;

namespace VlasikhaPlavanieWebsite.Application.Interfaces
{
	public interface ITinkoffWebhookService
	{
		Task<IActionResult> HandleWebhookAsync(TinkoffWebhookModel model);
	}
}