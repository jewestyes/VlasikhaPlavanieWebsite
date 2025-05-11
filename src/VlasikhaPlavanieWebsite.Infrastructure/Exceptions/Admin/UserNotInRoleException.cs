namespace VlasikhaPlavanieWebsite.Infrastructure.Exceptions.Admin
{
	public class UserNotInRoleException : Exception
	{
		public UserNotInRoleException(string email, string role)
			: base($"Пользователь \"{email}\" не в роли {role}.") { }
	}
}