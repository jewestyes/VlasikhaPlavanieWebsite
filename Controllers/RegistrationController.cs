using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using VlasikhaPlavanieWebsite.Models;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using VlasikhaPlavanieWebsite.Data;
using Microsoft.Extensions.Caching.StackExchangeRedis;

public class RegistrationController : Controller
{
	private readonly IDistributedCache _cache;
	private readonly ILogger<RegistrationController> _logger;
	private readonly ApplicationDbContext _context;

	public RegistrationController(IDistributedCache cache, ILogger<RegistrationController> logger, ApplicationDbContext context)
	{
		_cache = cache;
		_logger = logger;
		_context = context;
	}
	[HttpGet]
	public async Task<IActionResult> Index()
	{
		var registrationStage = await _context.RegistrationStage.FirstOrDefaultAsync(rs => rs.IsOpen);
		if (registrationStage == null)
		{
			_logger.LogWarning("No open registration stage found.");
			return Content("Не было найдено открытых регистраций.");
		}

		Dictionary<string, List<string>> options;

		options = await _context.StageDisciplines
			.Where(d => d.StageId == registrationStage.Id)
			.ToDictionaryAsync(
				d => d.Name,
				d => JsonSerializer.Deserialize<List<string>>(d.DistancesJson) ?? new List<string>()
		);

		var model = new RegistrationViewModel { DisciplineOptions = options, Stage = registrationStage, CompetitionDate = registrationStage.CompetitionDate, CompetitionAddress = registrationStage.CompetitionAddress };


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

		var orderId = Guid.NewGuid().ToString();
		_logger.LogInformation("Generated OrderId: {OrderId} for registration.", orderId);

		try
		{
			var cacheOptions = new DistributedCacheEntryOptions()
				.SetAbsoluteExpiration(TimeSpan.FromHours(3));

			int maxRetries = 10;
			for (int attempt = 0; attempt < maxRetries; attempt++)
			{
				try
				{
					await _cache.SetStringAsync(orderId, JsonSerializer.Serialize(model), cacheOptions);
					_logger.LogInformation("Registration data cached with OrderId: {OrderId}.", orderId);
					break;
				}
				catch (RedisException ex)
				{
					_logger.LogWarning(ex, "Ошибка при попытке записи в Redis, попытка {Attempt} из {MaxRetries}", attempt + 1, maxRetries);
					if (attempt == maxRetries - 1)
					{
						throw; // Если все попытки провалились, выбрасываем исключение
					}
					await Task.Delay(5000);
				}
			}

			return RedirectToAction("Payment", "Payment", new { orderId = orderId });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while submitting the registration with OrderId: {OrderId}.", orderId);
			return StatusCode(500, "Internal server error");
		}

	}
}
