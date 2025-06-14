using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VlasikhaPlavanieWebsite.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
	public ApplicationDbContext CreateDbContext(string[] args)
	{
		var path = Path.Combine(Directory.GetCurrentDirectory(), "..", "VlasikhaPlavanieWebsite.Api");

		if (!Directory.Exists(path))
			throw new DirectoryNotFoundException($"Cannot find Api project directory at {path}");

		var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

		var configuration = new ConfigurationBuilder()
			.SetBasePath(path)
			.AddJsonFile("appsettings.json", optional: false)
			.AddJsonFile($"appsettings.{environment}.json", optional: true)
			.Build();

		var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
		var connectionString = configuration.GetConnectionString("DefaultConnection");

		optionsBuilder.UseSqlServer(connectionString, b =>
			b.MigrationsAssembly("VlasikhaPlavanieWebsite.Infrastructure"));

		return new ApplicationDbContext(optionsBuilder.Options);
	}
}
