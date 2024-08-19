using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using VlasikhaPlavanieWebsite.Models;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

public class RegistrationController : Controller
{
    private readonly IDistributedCache _cache;

    public RegistrationController(IDistributedCache cache)
    {
        _cache = cache;
    }

    [HttpGet]
    public IActionResult Index()
    {
		var model = new RegistrationViewModel();

		foreach (var participant in model.Participants)
		{
			if (participant.Disciplines.Count == 0)
			{
				participant.Disciplines.Add(new Discipline());
			}
		}

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

        // Генерация уникального OrderId для использования на этапе оплаты
        var orderId = Guid.NewGuid().ToString();

        // Сохранение данных регистрации во временном хранилище
        var cacheOptions = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromHours(3));


        await _cache.SetStringAsync(orderId, JsonSerializer.Serialize(model), cacheOptions);


        // Перенаправление на страницу оплаты с передачей OrderId
        return RedirectToAction("Payment", "Payment", new { orderId = orderId });
    }

    [HttpGet("registration/get-cached-data")]
    public async Task<IActionResult> GetCachedData(string orderId)
    {
        var cachedData = await _cache.GetStringAsync(orderId);
        if (string.IsNullOrEmpty(cachedData))
        {
            return NotFound("Данные не найдены в кэше.");
        }

        var registrationModel = JsonSerializer.Deserialize<RegistrationViewModel>(cachedData);
        return Ok(registrationModel);
    }
}
