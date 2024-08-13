using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Threading.Tasks;
using VlasikhaPlavanieWebsite.Models;

namespace VlasikhaPlavanieWebsite.Controllers
{
    public class AdminController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("Admin/Login")]
        public IActionResult Login()
        {
            return View();
        }

		[HttpPost]
		[Route("Admin/Login")]
		public async Task<IActionResult> Login(LoginViewModel model)
		{
			if (!ModelState.IsValid)
			{
				Log.Warning("Invalid model state for login attempt. Email: {Email}, ModelState: {ModelStateErrors}",
							model.Email,
							string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
				return View(model);
			}

			try
			{
				Log.Information("Login attempt for user with email: {Email}", model.Email);

				var user = await _userManager.FindByEmailAsync(model.Email);
				if (user == null)
				{
					Log.Warning("No user found with email: {Email}", model.Email);
					ModelState.AddModelError("", "Invalid login attempt");
					return View(model);
				}

				if (!await _userManager.IsInRoleAsync(user, "Admin"))
				{
					Log.Warning("User {Email} is not in role Admin", model.Email);
					ModelState.AddModelError("", "Invalid login attempt");
					return View(model);
				}

				var checkPasswordResult = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
				if (!checkPasswordResult.Succeeded)
				{
					Log.Warning("Password verification failed for user: {Email}", model.Email);
					ModelState.AddModelError("", "Invalid login attempt");
					return View(model);
				}

				HttpContext.Session.Clear();
				await _signInManager.SignInAsync(user, false);
				Log.Information("User {Email} successfully signed in", model.Email);

				return RedirectToAction("Index", "Home");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "An error occurred while attempting to log in user: {Email}", model.Email);
				ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
				return View(model);
			}
		}


		[HttpGet]
		[Route("Admin/Logout")]
		public async Task<IActionResult> Logout()
		{
			var userName = User.Identity?.Name ?? "Unknown user";
			Log.Information("User {UserName} is attempting to log out.", userName);

			await _signInManager.SignOutAsync();
			Log.Information("User {UserName} has successfully logged out.", userName);

			foreach (var cookie in Request.Cookies.Keys)
			{
				Log.Information("Deleting cookie: {CookieName} for user {UserName}", cookie, userName);
				Response.Cookies.Delete(cookie);
			}

			Log.Information("All cookies deleted for user {UserName}.", userName);

			return RedirectToAction("Index", "Home");
		}
	}
}
