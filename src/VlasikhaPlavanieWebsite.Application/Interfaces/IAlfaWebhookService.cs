using Microsoft.AspNetCore.Mvc;
using VlasikhaPlavanieWebsite.Models;

namespace VlasikhaPlavanieWebsite.Application.Interfaces
{
	public interface IAlfaWebhookService
	{
		Task<IActionResult> HandleWebhookAsync(AlfaWebhook model);
	}
}