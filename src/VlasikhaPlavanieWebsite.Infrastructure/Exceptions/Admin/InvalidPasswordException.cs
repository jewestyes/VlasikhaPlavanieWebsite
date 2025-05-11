namespace VlasikhaPlavanieWebsite.Infrastructure.Exceptions.Admin
{
	public class InvalidPasswordException : Exception
	{
		public InvalidPasswordException(string email)
			: base($"Неправильный пароль для \"{email}\".") { }
	}
}
