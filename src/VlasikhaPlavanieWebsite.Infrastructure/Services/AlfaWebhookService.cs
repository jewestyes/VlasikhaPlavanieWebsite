using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using VlasikhaPlavanieWebsite.Application.Interfaces;
using VlasikhaPlavanieWebsite.Data;
using VlasikhaPlavanieWebsite.Models;

namespace VlasikhaPlavanieWebsite.Infrastructure.Services
{
	/// <summary>
	/// Обработка webhook от Альфа-Банка
	/// </summary>
	public class AlfaWebhookService : IAlfaWebhookService
	{
		private readonly ApplicationDbContext _db;
		private readonly ILogger<AlfaWebhookService> _logger;
		private readonly IConfiguration _configuration;
		private readonly IEmailQueue _emailQueue;

		public AlfaWebhookService(
			ApplicationDbContext db,
			ILogger<AlfaWebhookService> logger,
			IConfiguration configuration,
			IEmailQueue emailQueue)
		{
			_db = db;
			_logger = logger;
			_configuration = configuration;
			_emailQueue = emailQueue;
		}

		public async Task<IActionResult> HandleWebhookAsync(AlfaCallbackQuery model, IQueryCollection query)
		{
			_logger.LogInformation("Alfa callback received: orderNumber={Order}, mdOrder={MdOrder}, operation={Operation}, status={Status}",
				model.orderNumber, model.mdOrder, model.operation, model.status);

			bool isSuccess = string.Equals(model.status?.Trim(), "1", StringComparison.Ordinal);

			// Проверяем подпись
			if (!string.IsNullOrWhiteSpace(model.checksum))
			{
				if (!VerifySignature(query, model.checksum!))
				{
					_logger.LogError("Alfa callback checksum invalid. orderNumber={Order}, mdOrder={MdOrder}", model.orderNumber, model.mdOrder);
					return new OkObjectResult("OK");
				}
			}
			else
			{

				_logger.LogError("Alfa callback without checksum is not allowed. orderNumber={Order}, mdOrder={MdOrder}", model.orderNumber, model.mdOrder);
				return new OkObjectResult("OK");
			}

			// Финализируем оплату
			var operation = (model.operation ?? string.Empty).Trim().ToLowerInvariant();
			if (isSuccess && operation == "deposited")
			{
				// Попробуем валидировать сумму, если пришла в query как минимальные единицы (копейки).
				int? amountKopecks = null;
				if (int.TryParse((model.amount ?? string.Empty).Trim(), out var parsed))
					amountKopecks = parsed;

				return await HandleDepositedAsync(model, amountKopecks);
			}

			_logger.LogInformation("Alfa callback non-final or non-success. orderNumber={Order}, mdOrder={MdOrder}, operation={Operation}, status={Status}",
				model.orderNumber, model.mdOrder, model.operation, model.status);

			return new OkObjectResult("OK");
		}

		/// <summary>
		/// Верификация подписи по схеме HMAC-SHA256: исключаем checksum и sign_alias, сортируем по ключу,
		/// строим строку 'key;value;' и сравниваем с переданным checksum в верхнем регистре.
		/// </summary>
		private bool VerifySignature(IQueryCollection query, string checksum)
		{
			var secret = _configuration["Alfa:WebhookSecret"] ?? string.Empty;

			// Сформировать упорядоченную по имени параметров последовательность, исключив checksum и sign_alias
			var pairs = new SortedDictionary<string, string>(StringComparer.Ordinal);
			foreach (var kvp in query)
			{
				var key = kvp.Key ?? string.Empty;
				if (key.Equals("checksum", StringComparison.OrdinalIgnoreCase)) continue;
				if (key.Equals("sign_alias", StringComparison.OrdinalIgnoreCase)) continue;

				// Берем первое значение (Альфа присылает одиночные значения)
				var value = kvp.Value.ToString();
				pairs[key] = value ?? string.Empty;
			}

			var sb = new StringBuilder();
			foreach (var kv in pairs)
			{
				sb.Append(kv.Key).Append(';').Append(kv.Value).Append(';');
			}

			using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
			var payload = sb.ToString();
			var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
			var computedUpper = ConvertToHexLower(hash).ToUpperInvariant();

			return string.Equals(computedUpper, checksum.Trim(), StringComparison.OrdinalIgnoreCase);
		}

		private async Task<IActionResult> HandleDepositedAsync(AlfaCallbackQuery model, int? amountKopecks)
		{
			await using var transaction = await _db.Database.BeginTransactionAsync();

			try
			{
				var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderNumber == model.orderNumber);
				if (order == null)
				{
					_logger.LogError("Order not found (Alfa callback). orderNumber={Order}", model.orderNumber);
					return new BadRequestObjectResult("Заказ не найден.");
				}

				// Если сумма пришла в коллбеке — сверим (копейки).
				if (amountKopecks.HasValue)
				{
					var expectedKopecks = checked((int)(order.Amount * 100m));
					if (expectedKopecks != amountKopecks.Value)
					{
						_logger.LogError("Amount mismatch (Alfa callback). orderNumber={Order}, Expected={ExpectedKop}, Actual={ActualKop}",
							model.orderNumber, expectedKopecks, amountKopecks.Value);
						return new OkObjectResult("OK");
					}
				}

				// Идемпотентность
				if (order.Status == OrderStatus.Paid)
				{
					_logger.LogInformation("Order already Paid (idempotent OK). orderNumber={Order}", model.orderNumber);
					return new OkObjectResult("OK");
				}

				if (order.Status != OrderStatus.Pending)
				{
					_logger.LogWarning("Unexpected order status on Alfa deposited. orderNumber={Order}, Status={Status}", model.orderNumber, order.Status);
					return new OkObjectResult("OK");
				}

				order.Status = OrderStatus.Paid;
				order.UpdatedAt = DateTime.UtcNow;

				await _db.SaveChangesAsync();
				await transaction.CommitAsync();

				_emailQueue.EnqueuePaymentConfirmation(order.Id);

				_logger.LogInformation("Order marked as Paid (Alfa deposited). orderNumber={Order}, mdOrder={MdOrder}", model.orderNumber, model.mdOrder);
				return new OkObjectResult("OK");
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Exception during Alfa deposited handling. orderNumber={Order}", model.orderNumber);
				return new StatusCodeResult(500);
			}
		}

		private static string ConvertToHexLower(byte[] bytes)
		{
			var sb = new StringBuilder(bytes.Length * 2);

			for (int i = 0; i < bytes.Length; i++)
				sb.Append(bytes[i].ToString("x2"));

			return sb.ToString();
		}
	}
}
