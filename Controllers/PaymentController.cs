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

    [HttpGet]
    public IActionResult Payment(string orderId)
    {
        var order = _context.Orders
            .Include(o => o.Participants)
            .FirstOrDefault(o => o.OrderNumber == orderId);

        if (order == null)
        {
            return NotFound("Order not found.");
        }

        var firstParticipant = order.Participants.FirstOrDefault();
        if (firstParticipant == null)
        {
            return NotFound("No participants found in the order.");
        }

        var model = new PaymentViewModel
        {
            OrderId = order.Id,
            Amount = order.Amount,
            Description = "Оплата участия",
            Name = $"{firstParticipant.LastName} {firstParticipant.FirstName} {firstParticipant.MiddleName}",
            Email = firstParticipant.Email,
            Phone = firstParticipant.Phone
        };

        return View(model);
    }


    [HttpPost]
    public async Task<IActionResult> ProcessPayment(PaymentViewModel model)
    {
        // Преобразуем сумму в копейки без округления
        decimal amountInKopecksDecimal = model.Amount * 100;

        // Преобразуем сумму в целое число копеек
        int amountInKopecks = (int)amountInKopecksDecimal;

        // Проверка суммы
        if (amountInKopecks < 100)
        {
            ModelState.AddModelError(string.Empty, "Сумма должна быть не менее 1 рубля.");
            return View("Payment", model);
        }

        // Генерация токена с учетом всех обязательных параметров
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
            Amount = amountInKopecks.ToString(), // Передаем точное значение суммы в копейках
            OrderId = model.OrderId.ToString(),
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

            return Redirect(paymentResponse.PaymentURL);
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            ViewBag.ErrorMessage = "Произошла ошибка при обработке платежа: " + errorContent;
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
    }
}