using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VlasikhaPlavanieWebsite.Application.Interfaces;
using VlasikhaPlavanieWebsite.Models;
using VlasikhaPlavanieWebsite.ViewModels;
using VlasikhaPlavanieWebsite.Infrastructure.Exceptions.Payment;
using Microsoft.EntityFrameworkCore;
using VlasikhaPlavanieWebsite.Data;

namespace VlasikhaPlavanieWebsite.Infrastructure.Services.Payment
{
	public class PaymentService : IPaymentService
	{
		private readonly ApplicationDbContext _context;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;
		private readonly ILogger<PaymentService> _logger;
		public PaymentService(ApplicationDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<PaymentService> logger)
		{
			_context = context;
			_httpClientFactory = httpClientFactory;
			_configuration = configuration;
			_logger = logger;
		}


		public async Task<PaymentViewModel> GetPaymentInfoAsync(string orderId)
		{
			_logger.LogInformation("Начало обработки платежа для OrderId: {OrderId}", orderId);

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

			var amountKopecks = (int)(viewModel.Amount * 100);
			var token = GenerateToken(
				_configuration["Tinkoff:TerminalKey"],
				_configuration["Tinkoff:SecretKey"],
				amountKopecks.ToString(),
				viewModel.OrderId,
				viewModel.Description);

			return await SendInitRequestAsync(viewModel, amountKopecks, token);
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

		private async Task<string> SendInitRequestAsync(PaymentViewModel viewModel, int amountKopecks, string token)
		{
			var payload = new
			{
				TerminalKey = _configuration["Tinkoff:TerminalKey"],
				Amount = amountKopecks.ToString(),
				OrderId = viewModel.OrderId,
				Description = viewModel.Description,
				Name = viewModel.Name,
				Email = viewModel.Email,
				Phone = viewModel.Phone,
				Token = token
			};

			var client = _httpClientFactory.CreateClient();
			client.BaseAddress = new Uri(_configuration["Tinkoff:ApiUrl"]);
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
			var response = await client.PostAsync("Init", content);
			var raw = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
				throw new PaymentApiException(response.StatusCode, raw);

			var paymentResponse = JsonSerializer.Deserialize<PaymentResponse>(raw)!;
			if (!paymentResponse.Success)
				throw new PaymentInitializationException(paymentResponse.ErrorCode, paymentResponse.Message);

			return paymentResponse.PaymentURL;
		}

		private string GenerateToken(string terminalKey, string secretKey, string amount, string orderId, string description)
		{
			_logger.LogInformation("Генерация токена для OrderId: {OrderId}", orderId);

			var parameters = new SortedDictionary<string, string>
			{
				{ "TerminalKey", terminalKey },
				{ "Amount", amount },
				{ "OrderId", orderId },
				{ "Description", description },
				{ "Password", secretKey }
			};

			var concatenatedString = string.Join(string.Empty, parameters.Values);

			using (SHA256 sha256Hash = SHA256.Create())
			{
				byte[] sourceBytes = Encoding.UTF8.GetBytes(concatenatedString);
				byte[] hashBytes = sha256Hash.ComputeHash(sourceBytes);
				return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();
			}
		}

	}
}