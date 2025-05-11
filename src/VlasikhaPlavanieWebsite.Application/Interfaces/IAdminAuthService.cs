using Microsoft.AspNetCore.Http;
using VlasikhaPlavanieWebsite.ViewModels;

namespace VlasikhaPlavanieWebsite.Infrastructure.Services.Admin
{
	public interface IAdminAuthService
	{
		Task LoginAsync(LoginViewModel model, HttpContext httpContext);
		Task LogoutAsync(HttpContext httpContext);
	}
}