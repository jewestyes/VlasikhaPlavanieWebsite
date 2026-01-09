namespace VlasikhaPlavanieWebsite.Application.Interfaces
{
	public interface IEmailQueue
	{
		void EnqueuePaymentConfirmation(int orderId);
	}
}
