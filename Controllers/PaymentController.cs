using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using VlasikhaPlavanieWebsite.Data;
using VlasikhaPlavanieWebsite.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

public class PaymentController : Controller
{
	private readonly ApplicationDbContext _context;
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly IConfiguration _configuration;

	public PaymentController(ApplicationDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
	{
		_context = context;
		_httpClientFactory = httpClientFactory;
		_configuration = configuration;
	}

    [HttpGet("payment/success")]
    public IActionResult PaymentSuccess()
    {
        return View();
    }

    [HttpGet("payment/failure")]
    public IActionResult PaymentFailure()
    {
        return View();
    }

    [HttpGet]
	public IActionResult Payment(string orderId)
	{
		var registrationDataJson = HttpContext.Session.GetString("RegistrationData");
		var model = JsonSerializer.Deserialize<RegistrationViewModel>(registrationDataJson);

		if (model == null)
		{
			return NotFound("Не удалось найти данные для оплаты.");
		}

		var amount = decimal.Parse(HttpContext.Session.GetString("Amount"));

		var firstParticipant = model.Participants.FirstOrDefault();
		if (firstParticipant == null)
		{
			return NotFound("Не удалось найти участников.");
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

		return View(paymentViewModel);
	}

    [HttpPost]
    public async Task<IActionResult> ProcessPayment(PaymentViewModel model)
    {
        // Преобразуем сумму в копейки без округления
        decimal amountInKopecksDecimal = model.Amount * 100;
        int amountInKopecks = (int)amountInKopecksDecimal;

        string token = GenerateToken(
            _configuration["Tinkoff:TerminalKey"],
            amountInKopecks.ToString(),
            model.OrderId.ToString(),
            model.Description,
            _configuration["Tinkoff:SecretKey"]
        );

        var paymentData = new
        {
            TerminalKey = _configuration["Tinkoff:TerminalKey"],
            Amount = amountInKopecks.ToString(),
            OrderId = model.OrderId,
            Description = model.Description,
            Name = model.Name,
            Email = model.Email,
            Phone = model.Phone,
            Token = token
        };

        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(_configuration["Tinkoff:ApiUrl"]);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var content = new StringContent(JsonSerializer.Serialize(paymentData), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("Init", content);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var paymentResponse = JsonSerializer.Deserialize<PaymentResponse>(responseContent);

            if (paymentResponse.Success)
            {
                // Получение данных участников из сессии
                var registrationDataJson = HttpContext.Session.GetString("RegistrationData");
                var registrationModel = JsonSerializer.Deserialize<RegistrationViewModel>(registrationDataJson);

                if (registrationModel == null)
                {
                    ViewBag.ErrorMessage = "Не удалось восстановить данные участников из сессии.";
                    return View("PaymentError");
                }

                // Создаем новый заказ с участниками и статусом Pending
                var order = new Order
                {
                    OrderNumber = model.OrderId.ToString(),
                    Amount = model.Amount,
                    Participants = registrationModel.Participants,
                    Status = OrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                return Redirect(paymentResponse.PaymentURL);
            }
            else
            {
                ViewBag.ErrorMessage = $"Ошибка при обработке платежа: {paymentResponse.ErrorCode} - {paymentResponse.Message}";
                return View("PaymentError");
            }
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            ViewBag.ErrorMessage = $"Произошла ошибка при обработке платежа: {errorContent}";
            return View("PaymentError");
        }
    }

    private string GenerateToken(string terminalKey, string amount, string orderId, string description, string secretKey)
	{
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

	public class PaymentResponse
	{
		public bool Success { get; set; }
		public string ErrorCode { get; set; }
		public string TerminalKey { get; set; }
		public string Status { get; set; }
		public string PaymentId { get; set; }
		public string OrderId { get; set; }
		public int Amount { get; set; }
		public string PaymentURL { get; set; }
		public string Message { get; set; }

	}
}
