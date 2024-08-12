using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using VlasikhaPlavanieWebsite.Data;
using VlasikhaPlavanieWebsite.Models;
using Microsoft.EntityFrameworkCore;

namespace VlasikhaPlavanieWebsite.Controllers
{
    [Route("tinkoff")]
    public class TinkoffController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly ILogger<TinkoffController> _logger;

        public TinkoffController(ApplicationDbContext context, IDistributedCache cache, ILogger<TinkoffController> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> TinkoffWebhook([FromBody] TinkoffWebhookModel model)
        {
            _logger.LogInformation("Webhook received for OrderId: {OrderId} with status: {Status}", model.OrderId, model.Status);

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    if (model.Status == "CONFIRMED")
                    {
                        // Извлечение данных регистрации из временного хранилища
                        var registrationDataJson = await _cache.GetStringAsync(model.OrderId);
                        if (string.IsNullOrEmpty(registrationDataJson))
                        {
                            _logger.LogError("Ошибка: Данные регистрации не найдены в кеше.");
                            return BadRequest("Не удалось восстановить данные участников.");
                        }

                        var registrationModel = JsonSerializer.Deserialize<RegistrationViewModel>(registrationDataJson);

                        if (registrationModel == null)
                        {
                            _logger.LogError("Ошибка: Не удалось десериализовать данные регистрации.");
                            return BadRequest("Не удалось восстановить данные участников.");
                        }

                        var order = new Order
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

                        // Удаление данных из временного хранилища
                        await _cache.RemoveAsync(model.OrderId);
                        await _cache.RemoveAsync($"{model.OrderId}_amount");

                        await transaction.CommitAsync();
                    }
                    else
                    {
                        _logger.LogWarning($"Невозможно создать заказ. Статус: {model.Status}");
                        return BadRequest("Невозможно создать заказ без успешной оплаты.");
                    }

                    return Ok();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Ошибка обработки webhook для OrderId: {OrderId}", model.OrderId);
                    return StatusCode(500, "Ошибка обработки заказа.");
                }
            }
        }
    }
}
