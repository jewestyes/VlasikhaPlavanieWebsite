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
    private readonly ILogger<RegistrationController> _logger;

    public RegistrationController(IDistributedCache cache, ILogger<RegistrationController> logger)
    {
        _cache = cache;
        _logger = logger;
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
        _logger.LogInformation("Attempting to submit registration.");

        // Логирование всех данных формы
        try
        {
            string serializedModel = JsonSerializer.Serialize(model, new JsonSerializerOptions
            {
                WriteIndented = true, // Чтобы JSON был форматирован для лучшей читаемости
                IgnoreNullValues = false // Включаем все значения, включая null
            });

            _logger.LogInformation("Full registration data: {RegistrationData}", serializedModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while serializing the registration model.");
            return StatusCode(500, "Internal server error during serialization");
        }

        if (!ModelState.IsValid || model.Participants.Any(p => !p.Disciplines.Any()))
        {
            _logger.LogWarning("Registration model is invalid. Each participant must have at least one discipline.");
            ModelState.AddModelError(string.Empty, "У каждого участника должна быть выбрана хотя бы одна дисциплина.");
            return View("Index", model);
        }

        var orderId = Guid.NewGuid().ToString();
        _logger.LogInformation("Generated OrderId: {OrderId} for registration.", orderId);

        try
        {
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(3));

            await _cache.SetStringAsync(orderId, JsonSerializer.Serialize(model), cacheOptions);
            _logger.LogInformation("Registration data cached with OrderId: {OrderId}.", orderId);

            return RedirectToAction("Payment", "Payment", new { orderId = orderId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while submitting the registration with OrderId: {OrderId}.", orderId);
            return StatusCode(500, "Internal server error");
        }
    }
}
