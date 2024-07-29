using Microsoft.AspNetCore.Mvc;
using VlasikhaPlavanieWebsite.Data;
using VlasikhaPlavanieWebsite.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

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
        var order = _context.Orders.FirstOrDefault(o => o.OrderNumber == orderId);
        if (order == null)
        {
            return NotFound();
        }

        var model = new PaymentViewModel
        {
            OrderNumber = orderId,
            Amount = order.Amount
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ProcessPayment(PaymentViewModel model)
    {
        var order = _context.Orders.FirstOrDefault(o => o.OrderNumber == model.OrderNumber);
        if (order == null)
        {
            return NotFound();
        }

        var paymentRequest = new PaymentRequest
        {
            UserName = _configuration["AlphaBank:Login"],
            Password = _configuration["AlphaBank:Password"],
            OrderNumber = model.OrderNumber,
            Amount = (int)(model.Amount * 100), // Конвертируем сумму в копейки
            ReturnUrl = Url.Action("PaymentCallback", "Payment", new { orderId = model.OrderNumber }, Request.Scheme)
        };

        var httpClient = _httpClientFactory.CreateClient();
        var requestContent = new StringContent(JsonSerializer.Serialize(paymentRequest), Encoding.UTF8, "application/json");
        requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var response = await httpClient.PostAsync(_configuration["AlphaBank:PaymentUrl"], requestContent);

        if (!response.IsSuccessStatusCode)
        {
            return View("Failure");
        }

        var responseString = await response.Content.ReadAsStringAsync();
        var paymentResponse = JsonSerializer.Deserialize<PaymentResponse>(responseString);

        return Redirect(paymentResponse.FormUrl);
    }

    [HttpGet]
    public IActionResult PaymentCallback(string orderId, string status)
    {
        if (status == "success")
        {
            return View("Success");
        }
        else
        {
            return View("Failure");
        }
    }
}
