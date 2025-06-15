using Microsoft.EntityFrameworkCore;

internal class Program
{
	private static async Task Main(string[] args)
	{
		Console.WriteLine("Applying EF Core migrations...");

		var factory = new ApplicationDbContextFactory();
		using var db = factory.CreateDbContext(args);

		await db.Database.MigrateAsync();

		Console.WriteLine("Done.");
	}
}
