using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using VlasikhaPlavanieWebsite.Data;
using VlasikhaPlavanieWebsite.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace VlasikhaPlavanieWebsite.Controllers
{
    [Route("tinkoff")]
    public class TinkoffController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly ILogger<TinkoffController> _logger;
        private readonly IConfiguration _configuration;

        public TinkoffController(ApplicationDbContext context, IDistributedCache cache, ILogger<TinkoffController> logger, IConfiguration configuration)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> TinkoffWebhook([FromBody] TinkoffWebhookModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Webhook Invalid model state");
                return BadRequest(ModelState);
            }

            var existingOrder = await _context.Orders.FirstOrDefaultAsync(o => o.OrderNumber == model.OrderId);
            if (existingOrder != null)
            {
                _logger.LogInformation("Заказ с OrderId: {OrderId} уже существует. Запрос обработан ранее.", model.OrderId);
                return Ok();
            }


            _logger.LogInformation("Webhook received for OrderId: {OrderId} with status: {Status}", model.OrderId, model.Status);

            // Генерация токена на основе данных, пришедших в запросе
            string calculatedToken = GenerateTinkoffToken(model);

            // Проверка токена
            if (calculatedToken != model.Token)
            {
                _logger.LogWarning("Неверный токен для OrderId: {OrderId}.", model.OrderId);
                return BadRequest("Неверный токен.");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Проверка успешности и статуса платежа
                    if (model.Success && model.Status == "AUTHORIZED")
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
                        _logger.LogWarning($"Невозможно создать заказ. Статус: {model.Status}, Success: {model.Success}");
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

        private string GenerateTinkoffToken(TinkoffWebhookModel model)
        {
            var parameters = new SortedDictionary<string, string>
            {
                { "Amount", model.Amount.ToString() },
                { "CardId", model.CardId.ToString() },
                { "ErrorCode", model.ErrorCode },
                { "ExpDate", model.ExpDate },
                { "OrderId", model.OrderId },
                { "Pan", model.Pan },
                { "PaymentId", model.PaymentId.ToString() },
                { "Status", model.Status },
                { "Success", model.Success.ToString().ToLower() },
                { "TerminalKey", model.TerminalKey }
            };

            // Добавляем Password (секретный ключ)
            parameters.Add("Password", _configuration["Tinkoff:SecretKey"]);

            // Конкатенируем все значения в одну строку
            var concatenatedString = string.Join(string.Empty, parameters.Values);

            // Вычисляем SHA-256 хэш
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] sourceBytes = Encoding.UTF8.GetBytes(concatenatedString);
                byte[] hashBytes = sha256Hash.ComputeHash(sourceBytes);
                return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();
            }
        }
    }
}
