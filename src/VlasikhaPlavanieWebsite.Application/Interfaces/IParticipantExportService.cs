
namespace VlasikhaPlavanieWebsite.Infrastructure.Services.Admin
{
	public interface IParticipantExportService
	{
		Task<Stream> ExportByStageAsync(string stageName);
	}
}