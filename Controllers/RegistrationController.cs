using Microsoft.AspNetCore.Mvc;
using VlasikhaPlavanieWebsite.Models;
using System.Collections.Generic;

namespace VlasikhaPlavanieWebsite.Controllers
{
    public class RegistrationController : Controller
    {
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
        public IActionResult Submit(RegistrationViewModel model)
        {
            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
