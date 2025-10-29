using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VlasikhaPlavanieWebsite.Application.Interfaces;
using VlasikhaPlavanieWebsite.Data;
using VlasikhaPlavanieWebsite.Infrastructure.Exceptions.Payment;
using VlasikhaPlavanieWebsite.Models;
using VlasikhaPlavanieWebsite.ViewModels;

namespace VlasikhaPlavanieWebsite.Infrastructure.Services.Payment
{
	/// <summary>
	/// Интеграция с Альфа-Банк (redirect / платёжная страница банка).
	/// </summary>
	public class AlphaPaymentService : IPaymentService
	{
		private readonly ApplicationDbContext _context;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;
		private readonly ILogger<AlphaPaymentService> _logger;

		public AlphaPaymentService(
			ApplicationDbContext context,
			IHttpClientFactory httpClientFactory,
			IConfiguration configuration,
			ILogger<AlphaPaymentService> logger)
		{
			_context = context;
			_httpClientFactory = httpClientFactory;
			_configuration = configuration;
			_logger = logger;
		}

		public async Task<PaymentViewModel> GetPaymentInfoAsync(string orderId)
		{
			_logger.LogInformation("Начало обработки платежа (Alfa) для OrderId: {OrderId}", orderId);

			var order = await _context.Orders
				.Include(o => o.Participants)
				.ThenInclude(p => p.Disciplines)
				.FirstOrDefaultAsync(o => o.OrderNumber == orderId);

			if (order == null)
			{
				throw new PaymentDataNotFoundException(orderId);
			}

			var amount = order.Amount;

			var firstParticipant = order.Participants.FirstOrDefault();
			if (firstParticipant == null)
			{
				throw new ParticipantNotFoundException(orderId);
			}

			var paymentViewModel = new PaymentViewModel
			{
				OrderId = orderId,
				Amount = amount,
				Description = "Оплата участия",
				Name = $"{firstParticipant.LastName} {firstParticipant.FirstName} {firstParticipant.MiddleName}",
				Email = firstParticipant.Email,
				Phone = firstParticipant.Phone
			};

			return paymentViewModel;
		}

		public async Task<string> InitializePaymentAsync(PaymentViewModel viewModel)
		{
			_logger.LogInformation("Инициация платежа для OrderId={OrderId}", viewModel.OrderId);

			await ValidateOrderAsync(viewModel.OrderId, viewModel.Amount);

			var amountKopecks = checked((int)(viewModel.Amount * 100m));
			return await SendRegisterRequestAsync(viewModel, amountKopecks);
		}

		private async Task ValidateOrderAsync(string orderId, decimal providedAmount)
		{
			var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderId);
			if (order == null)
			{
				throw new PaymentDataNotFoundException(orderId);
			}

			if (order.Amount != providedAmount)
			{
				throw new PaymentAmountMismatchException(orderId, order.Amount, providedAmount);
			}
		}

		private async Task<string> SendRegisterRequestAsync(PaymentViewModel model, int amountKopecks)
		{
			var login = _configuration["Alfa:Login"];
			var password = _configuration["Alfa:Password"];
			var token = _configuration["Alfa:Token"];
			var apiUrl = _configuration["Alfa:ApiUrl"]?.TrimEnd('/') + "/register.do";

			var client = _httpClientFactory.CreateClient();
			client.BaseAddress = new Uri(apiUrl);
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


			var requestPairs = new List<KeyValuePair<string, string>>
			{
				// обязательные параметры заказа
				new KeyValuePair<string, string>("orderNumber", model.OrderId),
				new KeyValuePair<string, string>("amount", amountKopecks.ToString()),

				// описание и возвратные урлы
				new KeyValuePair<string, string>("description", model.Description ?? string.Empty),
				new KeyValuePair<string, string>("returnUrl", _configuration["Payment:ReturnSuccessUrl"] ?? "/payment/success"),
				new KeyValuePair<string, string>("failUrl", _configuration["Payment:ReturnFailUrl"] ?? "/payment/failure")
			};

			// аутентификация: если есть токен — используем его, иначе userName/password
			if (!string.IsNullOrWhiteSpace(token))
			{
				requestPairs.Add(new KeyValuePair<string, string>("token", token));
			}
			else
			{
				requestPairs.Add(new KeyValuePair<string, string>("userName", login ?? string.Empty));
				requestPairs.Add(new KeyValuePair<string, string>("password", password ?? string.Empty));
			}

			using var content = new FormUrlEncodedContent(requestPairs);
			var response = await client.PostAsync(string.Empty, content);
			var raw = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
			{
				throw new PaymentApiException(response.StatusCode, raw);
			}

			var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
			var registerResponse = JsonSerializer.Deserialize<AlfaRegisterResponse>(raw, options);

			if (registerResponse == null)
			{
				throw new PaymentInitializationException("NULL", "Не удалось разобрать ответ платёжного шлюза.");
			}

			if (!string.IsNullOrWhiteSpace(registerResponse.ErrorCode) && registerResponse.ErrorCode != "0")
			{
				throw new PaymentInitializationException(registerResponse.ErrorCode, registerResponse.ErrorMessage ?? "Ошибка регистрации заказа.");
			}

			if (string.IsNullOrWhiteSpace(registerResponse.FormUrl))
			{
				throw new PaymentInitializationException("NO_URL", "В ответе отсутствует URL платёжной страницы.");
			}

			return registerResponse.FormUrl;
		}
	}
}
