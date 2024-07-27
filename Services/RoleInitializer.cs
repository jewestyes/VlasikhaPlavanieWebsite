using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using VlasikhaPlavanieWebsite.Interfaces;
using VlasikhaPlavanieWebsite.Models;

namespace VlasikhaPlavanieWebsite.Services
{
    public class RoleInitializer : IRoleInitializer
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public RoleInitializer(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task Initialize()
        {
            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminEmail = _configuration["AppSettings:AdminUserEmail"];
            var adminPassword = _configuration["AppSettings:AdminUserPassword"];
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail
                };

                var result = await _userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(admin, "Admin");
                }
            }
            else
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(adminUser);
                await _userManager.ResetPasswordAsync(adminUser, token, adminPassword);
            }
        }
    }
}
