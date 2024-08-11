using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, false, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                ModelState.AddModelError("", "Invalid login attempt");
            }
            return View(model);
        }

        [HttpGet]
        [Route("Admin/Logout")]
        public async Task<IActionResult> Logout()
        {

            await _signInManager.SignOutAsync();


            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }


            return RedirectToAction("Index", "Home");

        }
    }
}
