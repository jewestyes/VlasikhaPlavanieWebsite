using Microsoft.AspNetCore.Mvc;
using VlasikhaPlavanieWebsite.ViewModels;

namespace VlasikhaPlavanieWebsite.Application.Interfaces
{
	public interface IRegistrationService
	{
		public Task<RegistrationViewModel> BuildIndexModelAsync();
		public Task<string> SubmitAsync(RegistrationViewModel viewModel);
	}
}