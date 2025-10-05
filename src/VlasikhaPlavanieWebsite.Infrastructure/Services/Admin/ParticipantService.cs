using Microsoft.EntityFrameworkCore;
using VlasikhaPlavanieWebsite.Application.Interfaces;
using VlasikhaPlavanieWebsite.Data;
using VlasikhaPlavanieWebsite.Models;
using VlasikhaPlavanieWebsite.ViewModels;

namespace VlasikhaPlavanieWebsite.Infrastructure.Services.Admin
{
	public class ParticipantService : IParticipantService
	{
		private readonly ApplicationDbContext _applicationDbContext;
		public ParticipantService(ApplicationDbContext applicationDbContext) => _applicationDbContext = applicationDbContext;

		public async Task<List<ParticipantOrderViewModel>> GetAllParticipantOrdersAsync()
		{
			var query = from p in _applicationDbContext.Participants
						join o in _applicationDbContext.Orders
							on p.OrderId equals o.Id into po
						from order in po.DefaultIfEmpty()
						join d in _applicationDbContext.Disciplines
							on p.Id equals d.ParticipantId into pd
						from discipline in pd.DefaultIfEmpty()
						join rs in _applicationDbContext.Competitions
							on order.CompetitionId equals rs.Id into ors
						from regcompetition in ors.DefaultIfEmpty()
						where order != null && order.Status == OrderStatus.Paid
						select new ParticipantOrderViewModel
						{
							LastName = p.LastName,
							FirstName = p.FirstName,
							MiddleName = p.MiddleName,
							BirthDate = p.BirthDate,
							Gender = p.Gender,
							CityOrTeam = p.CityOrTeam,
							Rank = p.Rank,
							Phone = p.Phone,
							CreatedAt = order.CreatedAt,
							Email = p.Email,
							DisciplineName = discipline != null ? discipline.Name : null,
							Distance = discipline != null ? discipline.Distance : null,
							EntryTime = discipline != null ? discipline.EntryTime : null,
							OrderNumber = order != null ? order.OrderNumber : null,
							Amount = order != null ? order.Amount : 0m,
							RegistrationCompetitionName = regcompetition != null ? regcompetition.Name : "Неизвестное название соревнования"
						};

			return await query.ToListAsync();
		}
	}
}