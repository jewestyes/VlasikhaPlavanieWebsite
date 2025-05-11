using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Serilog;
using VlasikhaPlavanieWebsite.Infrastructure.Exceptions.Admin;
using VlasikhaPlavanieWebsite.Models;
using VlasikhaPlavanieWebsite.ViewModels;


namespace VlasikhaPlavanieWebsite.Infrastructure.Services.Admin
{
	public class AdminAuthService : IAdminAuthService
	{
		private readonly SignInManager<ApplicationUser> _signIn;
		private readonly UserManager<ApplicationUser> _users;
		private readonly ILogger<AdminAuthService> _logger;

		private const string AdminRole = "Admin";

		public AdminAuthService(SignInManager<ApplicationUser> signIn,
								UserManager<ApplicationUser> users,
								ILogger<AdminAuthService> logger)
		{
			_signIn = signIn;
			_users = users;
			_logger = logger;
		}

		public async Task LoginAsync(LoginViewModel model, HttpContext httpContext)
		{
			Log.Information("Login attempt for user with email: {Email}", model.Email);

			var user = await ValidateAdminUserAsync(model.Email);
			await ValidatePasswordAsync(user, model.Password);

			httpContext.Session.Clear();
			await _signIn.SignInAsync(user, false);

			Log.Information("User {Email} successfully signed in", model.Email);
		}

		public async Task LogoutAsync(HttpContext httpContext)
		{
			await _signIn.SignOutAsync();
			httpContext.Session.Clear();
			foreach (var cookie in httpContext.Request.Cookies.Keys)
				httpContext.Response.Cookies.Delete(cookie);
		}

		private async Task<ApplicationUser?> ValidateAdminUserAsync(string email)
		{
			var user = await _users.FindByEmailAsync(email);
			if (user == null)
				throw new UserNotFoundException(email);

			if (!await _users.IsInRoleAsync(user, AdminRole))
				throw new UserNotInRoleException(email, AdminRole);

			return user;
		}

		private async Task ValidatePasswordAsync(ApplicationUser user, string password)
		{
			var result = await _signIn.CheckPasswordSignInAsync(user, password, false);
			if (!result.Succeeded)
				throw new InvalidPasswordException(user.Email);
		}
	}
}