using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using VlasikhaPlavanieWebsite.Application.Interfaces;
using VlasikhaPlavanieWebsite.Models;
using VlasikhaPlavanieWebsite.ViewModels;
using VlasikhaPlavanieWebsite.Data;

namespace VlasikhaPlavanieWebsite.Infrastructure.Services.Registration
{
    public class RegistrationService : IRegistrationService
    {
        private readonly ILogger<RegistrationService> _logger;
        private readonly ApplicationDbContext _context;

        public RegistrationService(ILogger<RegistrationService> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<RegistrationViewModel?> BuildIndexModelAsync(int id)
        {
            var competition = await _context.Competitions.FindAsync(id);
			if (competition == null || competition?.IsOpen == false)
			{
                return null;
            }

            Dictionary<string, List<string>> options;

            options = await _context.CompetitionDisciplines
                .Where(d => d.CompetitionId == competition.Id)
                .ToDictionaryAsync(
                    d => d.Name,
                    d => JsonSerializer.Deserialize<List<string>>(d.DistancesJson) ?? new List<string>()
            );

            var model = new RegistrationViewModel
            {
                DisciplineOptions = options,
                Competition = competition,
                CompetitionDate = competition.CompetitionStartDate,
                CompetitionAddress = competition.Address
            };


            foreach (var participant in model.Participants)
            {
                if (participant.Disciplines.Count == 0)
                {
                    participant.Disciplines.Add(new Discipline());
                }
            }

            return model;
        }

		public async Task<string> SubmitAsync(RegistrationViewModel viewModel)
		{
			var orderNumber = Guid.NewGuid().ToString();

            decimal totalPrice = CalculateCost(viewModel);

			var order = new Order
			{
				OrderNumber = orderNumber,
				Amount = totalPrice,
				Participants = viewModel.Participants,
				Status = OrderStatus.Pending,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow,
				CompetitionId = viewModel.Competition.Id
			};

			_context.Orders.Add(order);
			await _context.SaveChangesAsync();

			return orderNumber;
		}

		private decimal CalculateCost(RegistrationViewModel viewModel)
		{
			List<Participant> participants = viewModel.Participants;
			decimal totalPrice = 0m;

			foreach (Participant participant in participants)
			{
				int disciplinesCount = participant.Disciplines.Count();
				totalPrice += disciplinesCount <= 3 ? 2300m : 2300m + 500m * (disciplinesCount - 3);
			}

			return totalPrice;
		}
	}
}