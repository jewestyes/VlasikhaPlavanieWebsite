using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;
using VlasikhaPlavanieWebsite.Application.Interfaces;
using VlasikhaPlavanieWebsite.Data;
using VlasikhaPlavanieWebsite.Models;
using VlasikhaPlavanieWebsite.ViewModels;

namespace VlasikhaPlavanieWebsite.Infrastructure.Services
{
	public class TinkoffWebhookService : ITinkoffWebhookService
	{
		private readonly ApplicationDbContext _applicationDbContext;
		private readonly IDistributedCache _cache;
		private readonly ILogger<TinkoffWebhookService> _logger;
		private readonly IConfiguration _configuration;
		public TinkoffWebhookService(
			ApplicationDbContext context,
			IDistributedCache cache,
			ILogger<TinkoffWebhookService> logger,
			IConfiguration configuration)
		{
			_applicationDbContext = context;
			_cache = cache;
			_logger = logger;
			_configuration = configuration;
		}
		public async Task<IActionResult> HandleWebhookAsync(TinkoffWebhookModel model)
		{
			_logger.LogInformation("Webhook received for OrderId: {OrderId} with status: {Status}", model.OrderId, model.Status);

			var existingOrder = await _applicationDbContext.Orders.FirstOrDefaultAsync(o => o.OrderNumber == model.OrderId);
			if (existingOrder != null)
			{
				_logger.LogInformation("Order already exists: {OrderId}", model.OrderId);
				return new OkObjectResult("OK");
			}

			string calculatedToken = GenerateTinkoffToken(model);
			if (calculatedToken != model.Token)
			{
				_logger.LogError("Invalid token. Calculated: {Calc}, Provided: {Provided}, OrderId: {OrderId}", calculatedToken, model.Token, model.OrderId);
			}

			switch (model.Status)
			{
				case "CONFIRMED":
					return await HandleConfirmedAsync(model);

				case "AUTHORIZED":
				case "RESERVED":
				case "CANCELED":
				case "PARTIALLY_REFUNDED":
				case "REFUNDED":
				case "REVERSED":
					_logger.LogInformation("Status {Status} received for OrderId: {OrderId}", model.Status, model.OrderId);
					break;

				default:
					_logger.LogWarning("Unknown status: {Status} for OrderId: {OrderId}", model.Status, model.OrderId);
					break;
			}

			return new OkObjectResult("OK");
		}

		private async Task<IActionResult> HandleConfirmedAsync(TinkoffWebhookModel model)
		{
			using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();

			try
			{
				if (!model.Success)
				{
					_logger.LogWarning("Order not successful for OrderId: {OrderId}", model.OrderId);
					return new BadRequestObjectResult("Невозможно создать заказ без успешной оплаты.");
				}

				var registrationDataJson = await _cache.GetStringAsync(model.OrderId);
				if (string.IsNullOrEmpty(registrationDataJson))
				{
					_logger.LogError("Registration data not found in cache for OrderId: {OrderId}", model.OrderId);
					return new BadRequestObjectResult("Не удалось восстановить данные участников.");
				}

				var registrationModel = JsonSerializer.Deserialize<RegistrationViewModel>(registrationDataJson);
				if (registrationModel == null)
				{
					_logger.LogError("Failed to deserialize registration data for OrderId: {OrderId}", model.OrderId);
					return new BadRequestObjectResult("Не удалось восстановить данные участников.");
				}

				var order = new Order
				{
					OrderNumber = model.OrderId,
					Amount = model.Amount / 100m,
					Participants = registrationModel.Participants,
					Status = OrderStatus.Paid,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow,
					CompetitionId = registrationModel.Stage.Id,
				};

				_applicationDbContext.Orders.Add(order);
				await _applicationDbContext.SaveChangesAsync();

				await _cache.RemoveAsync(model.OrderId);
				await _cache.RemoveAsync($"{model.OrderId}_amount");

				await transaction.CommitAsync();
				_logger.LogInformation("Order {OrderId} created and cache cleared.", model.OrderId);
				return new OkObjectResult("OK");
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Exception during CONFIRMED handling for OrderId: {OrderId}", model.OrderId);
				return new StatusCodeResult(500);
			}
		}

		private string GenerateTinkoffToken(TinkoffWebhookModel model)
		{
			var parameters = new SortedDictionary<string, string>
			{
				{ "Amount", model.Amount.ToString() },
				{ "CardId", model.CardId.ToString() },
				{ "ErrorCode", model.ErrorCode },
				{ "ExpDate", model.ExpDate },
				{ "OrderId", model.OrderId },
				{ "Pan", model.Pan },
				{ "PaymentId", model.PaymentId.ToString() },
				{ "Status", model.Status },
				{ "Success", model.Success.ToString().ToLower() },
				{ "TerminalKey", model.TerminalKey },
				{ "Password", _configuration["Tinkoff:SecretKey"] }
			};

			var concatenated = string.Join(string.Empty, parameters.Values);

			using (SHA256 sha256Hash = SHA256.Create())
			{
				byte[] sourceBytes = Encoding.UTF8.GetBytes(concatenated);
				byte[] hashBytes = sha256Hash.ComputeHash(sourceBytes);
				return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();
			}
		}
	}
}