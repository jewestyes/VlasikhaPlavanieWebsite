using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VlasikhaPlavanieWebsite.Models;

namespace VlasikhaPlavanieWebsite.Application.Interfaces
{
	public interface IAlfaWebhookService
	{
		Task<IActionResult> HandleWebhookAsync(AlfaCallbackQuery model, IQueryCollection query);
	}
}
