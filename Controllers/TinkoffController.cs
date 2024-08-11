using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VlasikhaPlavanieWebsite.Data;
using VlasikhaPlavanieWebsite.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace VlasikhaPlavanieWebsite.Controllers
{
    [Route("tinkoff")]
    public class TinkoffController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TinkoffController> _logger;

        public TinkoffController(ApplicationDbContext context, ILogger<TinkoffController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> TinkoffWebhook([FromBody] TinkoffWebhookModel model)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderNumber == model.OrderId.ToString());

            if (order == null)
            {
                // Если заказ не найден, создаем новый заказ
                if (model.Status == "CONFIRMED")
                {
                    var registrationDataJson = HttpContext.Session.GetString("RegistrationData");
                    if (string.IsNullOrEmpty(registrationDataJson))
                    {
                        _logger.LogError("Ошибка: Данные регистрации не найдены в сессии.");
                        return BadRequest("Не удалось восстановить данные участников из сессии.");
                    }

                    var registrationModel = JsonSerializer.Deserialize<RegistrationViewModel>(registrationDataJson);

                    if (registrationModel == null)
                    {
                        _logger.LogError("Ошибка: Не удалось десериализовать данные регистрации.");
                        return BadRequest("Не удалось восстановить данные участников из сессии.");
                    }

                    order = new Order
                    {
                        OrderNumber = model.OrderId.ToString(),
                        Amount = model.Amount / 100m, // Преобразование суммы обратно в рубли
                        Participants = registrationModel.Participants,
                        Status = OrderStatus.Paid, // Статус успешной оплаты
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Заказ {order.OrderNumber} успешно создан.");
                }
                else
                {
                    _logger.LogWarning($"Невозможно создать заказ. Статус: {model.Status}");
                    return BadRequest("Невозможно создать заказ без успешной оплаты.");
                }
            }
            else
            {
                // Если заказ уже существует, обновляем его статус
                switch (model.Status)
                {
                    case "CONFIRMED":
                        order.Status = OrderStatus.Paid;
                        _logger.LogInformation($"Заказ {order.OrderNumber} подтвержден.");
                        break;
                    case "CANCELED":
                        order.Status = OrderStatus.Cancelled;
                        _logger.LogInformation($"Заказ {order.OrderNumber} отменен.");
                        break;
                    case "REJECTED":
                        order.Status = OrderStatus.Failed;
                        _logger.LogInformation($"Заказ {order.OrderNumber} отклонен.");
                        break;
                    default:
                        _logger.LogWarning($"Неизвестный статус: {model.Status} для заказа {order.OrderNumber}");
                        break;
                }

                order.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }
    }
}
