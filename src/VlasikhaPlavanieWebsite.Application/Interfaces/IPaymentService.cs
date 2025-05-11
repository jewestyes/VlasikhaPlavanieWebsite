
using VlasikhaPlavanieWebsite.ViewModels;

namespace VlasikhaPlavanieWebsite.Application.Interfaces
{
	public interface IPaymentService
	{
		public Task<PaymentViewModel> GetPaymentInfoAsync(string orderId);
		public Task<string> InitializePaymentAsync(PaymentViewModel viewModel);
	}
}