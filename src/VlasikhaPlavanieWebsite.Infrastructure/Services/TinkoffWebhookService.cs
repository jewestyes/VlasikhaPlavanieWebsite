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
		private readonly ILogger<TinkoffWebhookService> _logger;
		private readonly IConfiguration _configuration;
		private readonly IEmailService _emailService;
		public TinkoffWebhookService(
			ApplicationDbContext context,
			ILogger<TinkoffWebhookService> logger,
			IConfiguration configuration,
			IEmailService emailService)
		{
			_applicationDbContext = context;
			_logger = logger;
			_configuration = configuration;
			_emailService = emailService;
		}
		public async Task<IActionResult> HandleWebhookAsync(TinkoffWebhook model)
		{
			_logger.LogInformation("Webhook received for OrderId: {OrderId} with status: {Status}", model.OrderId, model.Status);

			var existingOrder = await _applicationDbContext.Orders.FirstOrDefaultAsync(o => o.OrderNumber == model.OrderId);


			string calculatedToken = GenerateTinkoffToken(model);
			if (calculatedToken != model.Token)
			{
				_logger.LogError("Invalid token. Calculated: {Calc}, Provided: {Provided}, OrderId: {OrderId}", calculatedToken, model.Token, model.OrderId);
				return new BadRequestObjectResult("Invalid token");
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

		private async Task<IActionResult> HandleConfirmedAsync(TinkoffWebhook model)
		{
			using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();

			try
			{
				if (!model.Success)
				{
					_logger.LogWarning("Order not successful for OrderId: {OrderId}", model.OrderId);
					return new BadRequestObjectResult("Оплата неуспешна.");
				}

				var order = await _applicationDbContext.Orders
					.FirstOrDefaultAsync(o => o.OrderNumber == model.OrderId);

				if (order == null)
				{
					_logger.LogError("Order not found in DB for OrderId: {OrderId}", model.OrderId);
					return new BadRequestObjectResult("Заказ не найден.");
				}

				var expectedKopecks = checked((int)(order.Amount * 100m));
				if (expectedKopecks != model.Amount)
				{
					_logger.LogError("Amount mismatch for OrderId: {OrderId}. Expected: {ExpectedKop}, Actual: {ActualKop}",
						model.OrderId, expectedKopecks, model.Amount);
					return new BadRequestObjectResult("Несоответствие суммы.");
				}

				if (order.Status == OrderStatus.Paid)
				{
					_logger.LogInformation("Order {OrderId} already paid. Idempotent OK.", model.OrderId);
					return new OkObjectResult("OK");
				}

				if (order.Status != OrderStatus.Pending)
				{
					_logger.LogWarning("Order {OrderId} has unexpected status {Status} on CONFIRMED.", model.OrderId, order.Status);
					return new OkObjectResult("OK");
				}

				order.Status = OrderStatus.Paid;
				order.UpdatedAt = DateTime.UtcNow;

				await _applicationDbContext.SaveChangesAsync();
				await transaction.CommitAsync();

				try
				{
					await _emailService.SendPaymentConfirmationAsync(order.Id);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Failed to send payment confirmation email for OrderId: {OrderId}", model.OrderId);
				}

				_logger.LogInformation("Order {OrderId} marked as Paid.", model.OrderId);
				return new OkObjectResult("OK");
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Exception during CONFIRMED handling for OrderId: {OrderId}", model.OrderId);
				return new StatusCodeResult(500);
			}
		}


		private string GenerateTinkoffToken(TinkoffWebhook model)
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
