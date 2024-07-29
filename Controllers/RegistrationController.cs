using Microsoft.AspNetCore.Mvc;
using VlasikhaPlavanieWebsite.Data;
using VlasikhaPlavanieWebsite.Models;
using System.Linq;
using System;
using System.Threading.Tasks;

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
        if (!ModelState.IsValid || model.Participants.Any(p => !p.Disciplines.Any()))
        {
            ModelState.AddModelError(string.Empty, "У каждого участника должна быть выбрана хотя бы одна дисциплина.");
            return View("Index", model);
        }

        foreach (var participant in model.Participants)
        {
            _context.Participants.Add(participant);
        }

        await _context.SaveChangesAsync();

        var orderId = Guid.NewGuid().ToString();
        var amount = CalculateCost(model.Participants.Sum(p => p.Disciplines.Count));

        var order = new Order
        {
            OrderNumber = orderId,
            Amount = amount,
            Participants = model.Participants
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return RedirectToAction("Payment", "Payment", new { orderId = orderId });
    }

    private decimal CalculateCost(int disciplinesCount)
    {
        return ((disciplinesCount / 3) * 2000m) + ((disciplinesCount % 3 != 0) ? 2000m : 0m);
    }
}
