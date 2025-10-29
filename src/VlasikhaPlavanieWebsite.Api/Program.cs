using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.DataProtection;
using VlasikhaPlavanieWebsite.Data;
using VlasikhaPlavanieWebsite.Interfaces;
using VlasikhaPlavanieWebsite.Models;
using VlasikhaPlavanieWebsite.Services;
using VlasikhaPlavanieWebsite.Application.Interfaces;
using VlasikhaPlavanieWebsite.Infrastructure.Services.Registration;
using VlasikhaPlavanieWebsite.Infrastructure.Services.Admin;
using VlasikhaPlavanieWebsite.Infrastructure.Services;
using VlasikhaPlavanieWebsite.Infrastructure.Services.Payment;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

var supportedCultures = new[] { new CultureInfo("ru-RU") };

builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Login";
});

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("Session:IdleTimeoutMinutes"));
    options.Cookie.Name = builder.Configuration.GetValue<string>("Session:CookieName");
    options.Cookie.HttpOnly = builder.Configuration.GetValue<bool>("Session:CookieHttpOnly");
    options.Cookie.IsEssential = builder.Configuration.GetValue<bool>("Session:CookieIsEssential");
});


builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();
builder.Services.AddScoped<IParticipantExportService, ParticipantExportService>();
builder.Services.AddScoped<IParticipantService, ParticipantService>();
builder.Services.AddScoped<ICompetitionService, CompetitionService>();
builder.Services.AddScoped<IFileMappingService, FileMappingService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IPaymentService, AlphaPaymentService>();
builder.Services.AddScoped<IAlfaWebhookService, AlfaWebhookService>();
//builder.Services.AddScoped<IPaymentService, TinkoffPaymentService>();
//builder.Services.AddScoped<ITinkoffWebhookService, TinkoffWebhookService>();
builder.Services.AddScoped<IStatService, StatService>();
builder.Services.AddRazorPages();
builder.Services.AddTransient<IRoleInitializer, RoleInitializer>();
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseRequestLocalization(new RequestLocalizationOptions
{
	DefaultRequestCulture = new RequestCulture("ru-RU"),
	SupportedCultures = supportedCultures,
	SupportedUICultures = supportedCultures
});


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	var context = services.GetRequiredService<ApplicationDbContext>();

	var env = services.GetRequiredService<IWebHostEnvironment>();

	var roleInitializer = services.GetRequiredService<IRoleInitializer>();
	roleInitializer.Initialize().Wait();
}

app.Run();
