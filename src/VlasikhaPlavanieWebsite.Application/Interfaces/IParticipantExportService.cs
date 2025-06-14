
namespace VlasikhaPlavanieWebsite.Infrastructure.Services.Admin
{
	public interface IParticipantExportService
	{
		Task<Stream> ExportBycompetitionAsync(string competitionName);
	}
}