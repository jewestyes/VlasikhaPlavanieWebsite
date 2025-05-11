namespace VlasikhaPlavanieWebsite.Infrastructure.Exceptions.Admin
{
	public class UserNotFoundException : Exception
	{
		public UserNotFoundException(string email)
			: base($"Пользователь с email \"{email}\" не найден.") { }
	}
}