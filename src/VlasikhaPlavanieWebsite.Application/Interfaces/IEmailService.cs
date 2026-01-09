using System.Threading.Tasks;

namespace VlasikhaPlavanieWebsite.Application.Interfaces
{
	public interface IEmailService
	{
		Task SendPaymentConfirmationAsync(int orderId);
	}
}
