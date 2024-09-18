using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StackExchange.Redis;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.StackExchangeRedis;
using VlasikhaPlavanieWebsite.Data;
using VlasikhaPlavanieWebsite.Interfaces;
using VlasikhaPlavanieWebsite.Models;
using VlasikhaPlavanieWebsite.Services;

var builder = WebApplication.CreateBuilder(args);

//test
// Загрузка конфигурации из файла appsettings.json
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

// Настройка Serilog для логирования
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

// Подключение к Redis для сессий
var redisConnection = builder.Configuration.GetConnectionString("RedisConnection");
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "VlasikhaPlavanieWebsite_";
});

// Настройка сессий с использованием Redis
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("Session:IdleTimeoutMinutes"));
    options.Cookie.Name = builder.Configuration.GetValue<string>("Session:CookieName");
    options.Cookie.HttpOnly = builder.Configuration.GetValue<bool>("Session:CookieHttpOnly");
    options.Cookie.IsEssential = builder.Configuration.GetValue<bool>("Session:CookieIsEssential");
});

// Настройка Data Protection
var dataProtectionConfig = builder.Configuration.GetSection("DataProtection");
if (dataProtectionConfig.GetValue<bool>("UseRedis"))
{
    builder.Services.AddDataProtection()
        .PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect(redisConnection), dataProtectionConfig.GetValue<string>("RedisKey"))
        .SetApplicationName(dataProtectionConfig.GetValue<string>("ApplicationName"));
}
else if (dataProtectionConfig.GetValue<bool>("PersistKeysToFileSystem"))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionConfig.GetValue<string>("KeyFilePath")))
        .SetApplicationName(dataProtectionConfig.GetValue<string>("ApplicationName"));
}

if (dataProtectionConfig.GetValue<bool>("ProtectKeysWithCertificate"))
{
    builder.Services.AddDataProtection()
        .ProtectKeysWithCertificate(dataProtectionConfig.GetValue<string>("CertificateThumbprint"));
}

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddTransient<IRoleInitializer, RoleInitializer>();
builder.Services.AddHttpClient();

var app = builder.Build();

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
    context.Database.Migrate();
    var roleInitializer = services.GetRequiredService<IRoleInitializer>();
    roleInitializer.Initialize().Wait();
}

app.Run();
