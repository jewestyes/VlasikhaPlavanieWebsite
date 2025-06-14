using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using VlasikhaPlavanieWebsite.ViewModels;
using VlasikhaPlavanieWebsite.Application.Interfaces;
using VlasikhaPlavanieWebsite.Controllers;

public class RegistrationController : BaseController
{
	private readonly ILogger<RegistrationController> _logger;
	private readonly IRegistrationService _registrationService;

	public RegistrationController(IRegistrationService registrationService, ILogger<RegistrationController> logger, IHomeService homeService)
			: base(homeService)
	{
		_registrationService = registrationService;
		_logger = logger;
	}
	[HttpGet]
	public async Task<IActionResult> Index()
	{
		var model = await _registrationService.BuildIndexModelAsync();
		if (model == null)
		{
			_logger.LogWarning("No open registration stage found.");
			return Content("Не было найдено открытых регистраций.");
		}

		return View(model);
	}


	[HttpPost]
	public async Task<IActionResult> Submit(RegistrationViewModel model)
	{
		_logger.LogInformation("Attempting to submit registration.");


		ModelState.Remove("Stage.CompetitionAddress");

		for (int i = 0; i < model.Participants.Count; i++)
		{
			ModelState.Remove($"Participants[{i}].Order");
			ModelState.Remove($"Participants[{i}].Rank");
			ModelState.Remove($"Participants[{i}].OrderId");
		}

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


		try
		{
			var orderId = await _registrationService.SubmitAsync(model);

			return RedirectToAction("Payment", "Payment", new { orderId = orderId });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while submitting the registration");
			return StatusCode(500, "Internal server error");
		}
	}
}