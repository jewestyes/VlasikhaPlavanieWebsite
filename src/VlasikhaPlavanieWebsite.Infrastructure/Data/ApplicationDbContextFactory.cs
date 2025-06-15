using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VlasikhaPlavanieWebsite.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
	public ApplicationDbContext CreateDbContext(string[] args)
	{
		string path;

#if DEBUG
		path = Path.Combine(Directory.GetCurrentDirectory(), "..", "VlasikhaPlavanieWebsite.Api");
#else
		path = Directory.GetCurrentDirectory();
#endif

		if (!Directory.Exists(path))
			throw new DirectoryNotFoundException($"Cannot find configuration directory at {path}");

		var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

		var configuration = new ConfigurationBuilder()
			.SetBasePath(path)
			.AddJsonFile("appsettings.json", optional: true)
			.AddJsonFile($"appsettings.{environment}.json", optional: true)
			.AddJsonFile("appsettings.Production.json", optional: true)
			.Build();

		var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
		var connectionString = configuration.GetConnectionString("DefaultConnection");

		optionsBuilder.UseSqlServer(connectionString, b =>
			b.MigrationsAssembly("VlasikhaPlavanieWebsite.Infrastructure"));

		return new ApplicationDbContext(optionsBuilder.Options);
	}
}
