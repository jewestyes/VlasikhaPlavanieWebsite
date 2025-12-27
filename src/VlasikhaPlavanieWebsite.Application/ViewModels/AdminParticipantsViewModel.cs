using System;
using System.Collections.Generic;

namespace VlasikhaPlavanieWebsite.ViewModels
{
    public class AdminParticipantsViewModel
    {
        public List<ParticipantOrderViewModel> Participants { get; set; } = new List<ParticipantOrderViewModel>();
        public List<CompetitionSummaryViewModel> Competitions { get; set; } = new List<CompetitionSummaryViewModel>();

        // Название соревнования, которое должно быть выбрано по умолчанию при загрузке страницы
        public string SelectedCompetitionName { get; set; }
    }

    public class CompetitionSummaryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsOpen { get; set; }
        public DateTime? RegistrationEndDate { get; set; }
    }
}