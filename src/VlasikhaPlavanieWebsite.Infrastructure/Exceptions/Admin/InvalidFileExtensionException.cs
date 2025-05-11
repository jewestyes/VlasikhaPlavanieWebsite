
namespace VlasikhaPlavanieWebsite.Infrastructure.Exceptions.Admin
{
	public class InvalidFileExtensionException : Exception
	{
		public InvalidFileExtensionException(string fileName)
			: base($"Недопустимый формат файла: {fileName}") { }
	}
}