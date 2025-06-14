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
            var stage = await _context.RegistrationStage.FirstOrDefaultAsync(rs => rs.IsOpen);
            if (stage == null)
            {
                return null;
            }

            Dictionary<string, List<string>> options;

            options = await _context.StageDisciplines
                .Where(d => d.CompetitionId == stage.Id)
                .ToDictionaryAsync(
                    d => d.Name,
                    d => JsonSerializer.Deserialize<List<string>>(d.DistancesJson) ?? new List<string>()
            );

            var model = new RegistrationViewModel
            {
                DisciplineOptions = options,
                Stage = stage,
                CompetitionDate = stage.CompetitionStartDate,
                CompetitionAddress = stage.Address
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