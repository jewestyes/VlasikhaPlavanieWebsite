using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using VlasikhaPlavanieWebsite.Models;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

public class PaymentController : Controller
{
    private readonly IDistributedCache _cache;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(IDistributedCache cache, IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<PaymentController> logger)
    {
        _cache = cache;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
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
    public async Task<IActionResult> Payment(string orderId)
    {
        _logger.LogInformation("Начало обработки платежа для OrderId: {OrderId}", orderId);

        var registrationDataJson = await _cache.GetStringAsync(orderId);
        if (registrationDataJson == null)
        {
            _logger.LogWarning("Не удалось найти данные для оплаты по OrderId: {OrderId}", orderId);
            return NotFound("Не удалось найти данные для оплаты.");
        }

        var model = JsonSerializer.Deserialize<RegistrationViewModel>(registrationDataJson);

		decimal amount = CalculateCost(model.Participants.Sum(p => p.Disciplines.Count));

        var firstParticipant = model.Participants.FirstOrDefault();
        if (firstParticipant == null)
        {
            _logger.LogWarning("Не удалось найти участников для OrderId: {OrderId}", orderId);
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
        _logger.LogInformation("Обработка платежа для OrderId: {OrderId}", model.OrderId);

        // Преобразуем сумму в копейки
        decimal amountInKopecksDecimal = model.Amount * 100;
        int amountInKopecks = (int)amountInKopecksDecimal;

        string token;
        try
        {
            token = GenerateToken(
                _configuration["Tinkoff:TerminalKey"],
                amountInKopecks.ToString(),
                model.OrderId.ToString(),
                model.Description,
                _configuration["Tinkoff:SecretKey"]
            );
            _logger.LogInformation("Токен для OrderId: {OrderId} успешно сгенерирован", model.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка генерации токена для OrderId: {OrderId}", model.OrderId);
            return View("PaymentError");
        }

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
                _logger.LogInformation("Платеж для OrderId: {OrderId} успешно инициализирован", model.OrderId);
                return Redirect(paymentResponse.PaymentURL);
            }
            else
            {
                _logger.LogWarning("Ошибка инициализации платежа для OrderId: {OrderId}. Код ошибки: {ErrorCode}, Сообщение: {Message}", paymentResponse.OrderId, paymentResponse.ErrorCode, paymentResponse.Message);
                ViewBag.ErrorMessage = $"Ошибка при обработке платежа: {paymentResponse.ErrorCode} - {paymentResponse.Message}";
                return View("PaymentFailure");
            }
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Ошибка при обращении к API Tinkoff для OrderId: {OrderId}. Ответ сервера: {ResponseContent}", model.OrderId, errorContent);
            ViewBag.ErrorMessage = $"Произошла ошибка при обработке платежа: {errorContent}";
            return View("PaymentFailure");
        }
    }

    private string GenerateToken(string terminalKey, string amount, string orderId, string description, string secretKey)
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

	private decimal CalculateCost(int disciplinesCount)
	{
		return disciplinesCount <= 3 ? 2000m : 2000m + 500m * (disciplinesCount - 3);
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
