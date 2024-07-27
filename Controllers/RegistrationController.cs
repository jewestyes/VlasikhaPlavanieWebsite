using Microsoft.AspNetCore.Mvc;
using VlasikhaPlavanieWebsite.Models;
using VlasikhaPlavanieWebsite.Data;
using System.Threading.Tasks;
using System.Linq;

namespace VlasikhaPlavanieWebsite.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegistrationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new RegistrationViewModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult AddParticipant(RegistrationViewModel model)
        {
            model.Participants.Add(new Participant());
            return View("Index", model);
        }

        [HttpPost]
        public IActionResult AddDiscipline(RegistrationViewModel model, int participantIndex)
        {
            model.Participants[participantIndex].Disciplines.Add(new Discipline());
            return View("Index", model);
        }

        [HttpPost]
        public IActionResult RemoveDiscipline(RegistrationViewModel model, int participantIndex, int disciplineIndex)
        {
            if (participantIndex >= 0 && participantIndex < model.Participants.Count)
            {
                var participant = model.Participants[participantIndex];
                if (disciplineIndex >= 0 && disciplineIndex < participant.Disciplines.Count)
                {
                    participant.Disciplines.RemoveAt(disciplineIndex);
                }
            }
            return View("Index", model);
        }

        [HttpPost]
        public IActionResult RemoveParticipant(RegistrationViewModel model, int participantIndex)
        {
            if (participantIndex >= 0 && participantIndex < model.Participants.Count)
            {
                model.Participants.RemoveAt(participantIndex);
            }
            return View("Index", model);
        }

        [HttpPost]
        public async Task<IActionResult> Submit(RegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            foreach (var participant in model.Participants)
            {
                _context.Participants.Add(participant);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
