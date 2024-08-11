using Microsoft.AspNetCore.Mvc;
using VlasikhaPlavanieWebsite.Data;
using VlasikhaPlavanieWebsite.Models;
using Microsoft.EntityFrameworkCore;

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
                return NotFound();
            }

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
                    // Обработка других статусов
                    break;
            }

            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }

}
