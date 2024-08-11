using Microsoft.AspNetCore.Mvc;
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

        public TinkoffController(ApplicationDbContext context)
        {
            _context = context;
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
                    var registrationModel = JsonSerializer.Deserialize<RegistrationViewModel>(registrationDataJson);

                    if (registrationModel == null)
                    {
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
                }
                else
                {
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
                        break;
                    case "CANCELED":
                        order.Status = OrderStatus.Cancelled;
                        break;
                    case "REJECTED":
                        order.Status = OrderStatus.Failed;
                        break;
                    default:
                        break;
                }

                order.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

    }

}
