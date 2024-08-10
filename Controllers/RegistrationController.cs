using Microsoft.AspNetCore.Mvc;
using VlasikhaPlavanieWebsite.Data;
using VlasikhaPlavanieWebsite.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

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
	public IActionResult Submit(RegistrationViewModel model)
	{
		if (!ModelState.IsValid || model.Participants.Any(p => !p.Disciplines.Any()))
		{
			ModelState.AddModelError(string.Empty, "У каждого участника должна быть выбрана хотя бы одна дисциплина.");
			return View("Index", model);
		}

		// Сохранение данных регистрации в сессии
		HttpContext.Session.SetString("RegistrationData", JsonSerializer.Serialize(model));

		// Генерация уникального OrderId для использования на этапе оплаты
		var orderId = Guid.NewGuid().ToString();
		var amount = CalculateCost(model.Participants.Sum(p => p.Disciplines.Count));

		// Сохранение данных заказа в сессии
		HttpContext.Session.SetString("OrderId", orderId);
		HttpContext.Session.SetString("Amount", amount.ToString());

		// Перенаправление на страницу оплаты с передачей OrderId
		return RedirectToAction("Payment", "Payment", new { orderId = orderId });
	}


	private decimal CalculateCost(int disciplinesCount)
	{
		return ((disciplinesCount / 3) * 2000m) + ((disciplinesCount % 3 != 0) ? 2000m : 0m);
	}
}
