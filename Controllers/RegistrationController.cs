using Microsoft.AspNetCore.Mvc;
using VlasikhaPlavanieWebsite.Data;
using VlasikhaPlavanieWebsite.Models;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;

public class RegistrationController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public RegistrationController(ApplicationDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var model = new RegistrationViewModel();
        return View(model);
    }

    [HttpPost]
    public IActionResult AddParticipant(RegistrationViewModel model)
    {
        model.Participants.Add(new Participant());
        return View("Index", model);
    }

    [HttpPost]
    public IActionResult AddDiscipline(RegistrationViewModel model, int participantIndex)
    {
        model.Participants[participantIndex].Disciplines.Add(new Discipline());
        return View("Index", model);
    }

    [HttpPost]
    public IActionResult RemoveDiscipline(RegistrationViewModel model, int participantIndex, int disciplineIndex)
    {
        if (participantIndex >= 0 && participantIndex < model.Participants.Count)
        {
            var participant = model.Participants[participantIndex];
            if (disciplineIndex >= 0 && disciplineIndex < participant.Disciplines.Count)
            {
                participant.Disciplines.RemoveAt(disciplineIndex);
            }
        }
        return View("Index", model);
    }

    [HttpPost]
    public IActionResult RemoveParticipant(RegistrationViewModel model, int participantIndex)
    {
        if (participantIndex >= 0 && participantIndex < model.Participants.Count)
        {
            model.Participants.RemoveAt(participantIndex);
        }
        return View("Index", model);
    }

    [HttpPost]
    public async Task<IActionResult> Submit(RegistrationViewModel model)
    {
        if (!ModelState.IsValid || model.Participants.Any(p => !p.Disciplines.Any()))
        {
            ModelState.AddModelError(string.Empty, "У каждого участника должна быть выбрана хотя бы одна дисциплина.");
            return View("Index", model);
        }

        foreach (var participant in model.Participants)
        {
            _context.Participants.Add(participant);
        }

        await _context.SaveChangesAsync();

        var orderId = Guid.NewGuid().ToString();
        var amount = (int)(CalculateCost(model.Participants.Sum(p => p.Disciplines.Count)) * 100);

        var paymentRequest = new PaymentRequest
        {
            UserName = _configuration["AlphaBank:Login"],
            Password = _configuration["AlphaBank:Password"],
            OrderNumber = orderId,
            Amount = amount,
            ReturnUrl = Url.Action("PaymentCallback", "Registration", new { orderId }, Request.Scheme)
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

    private decimal CalculateCost(int disciplinesCount)
    {
        return ((disciplinesCount / 3) * 2000m) + ((disciplinesCount % 3 != 0) ? 2000m : 0m);
    }
}
