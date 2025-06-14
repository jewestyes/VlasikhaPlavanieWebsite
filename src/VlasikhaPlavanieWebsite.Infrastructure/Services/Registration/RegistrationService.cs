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
        private readonly IRegistrationCache _registrationCache;
        private readonly ILogger<RegistrationService> _logger;
        private readonly ApplicationDbContext _context;

        public RegistrationService(ILogger<RegistrationService> logger, ApplicationDbContext context, IRegistrationCache registrationCache)
        {
            _logger = logger;
            _context = context;
            _registrationCache = registrationCache;
        }

        public async Task<RegistrationViewModel?> BuildIndexModelAsync()
        {
            var competition = await _context.Competitions.FirstOrDefaultAsync(rs => rs.IsOpen);
            if (competition == null)
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
            var orderId = await _registrationCache.CacheAsync(viewModel);

            return orderId;
        }
    }
}