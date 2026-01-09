using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VlasikhaPlavanieWebsite.Application.Interfaces;
using VlasikhaPlavanieWebsite.Data;

namespace VlasikhaPlavanieWebsite.Infrastructure.Services.Email
{
	public class EmailService : IEmailService
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly IConfiguration _configuration;
		private readonly ILogger<EmailService> _logger;

		public EmailService(ApplicationDbContext dbContext, IConfiguration configuration, ILogger<EmailService> logger)
		{
			_dbContext = dbContext;
			_configuration = configuration;
			_logger = logger;
		}

		public async Task SendPaymentConfirmationAsync(int orderId)
		{
			var order = await _dbContext.Orders
				.AsNoTracking()
				.Include(o => o.Competition)
				.Include(o => o.Participants)
				.ThenInclude(p => p.Disciplines)
				.FirstOrDefaultAsync(o => o.Id == orderId);

			if (order == null)
			{
				_logger.LogWarning("Payment confirmation email skipped because order not found. OrderId={OrderId}", orderId);
				return;
			}

			var recipients = order.Participants
				.Select(p => p.Email)
				.Where(email => !string.IsNullOrWhiteSpace(email))
				.Select(email => email.Trim())
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();

			if (recipients.Count == 0)
			{
				_logger.LogWarning("Payment confirmation email skipped because no recipient emails found. OrderId={OrderId}", orderId);
				return;
			}

			var fromAddress = _configuration["Email:FromAddress"];
			var smtpHost = _configuration["Email:SmtpHost"];
			var smtpPortValue = _configuration["Email:SmtpPort"];
			var smtpUser = _configuration["Email:User"];
			var smtpPassword = _configuration["Email:Password"];
			var enableSslValue = _configuration["Email:EnableSsl"];

			if (string.IsNullOrWhiteSpace(fromAddress) || string.IsNullOrWhiteSpace(smtpHost))
			{
				_logger.LogWarning("Email settings are missing; skipping confirmation email. OrderId={OrderId}", orderId);
				return;
			}

			var subject = $"Оплата подтверждена: {order.Competition?.Name ?? "Регистрация"}";
			var body = BuildPaymentConfirmationBody(order.Competition?.Name, order.Participants);

			var port = int.TryParse(smtpPortValue, out var parsedPort) ? parsedPort : 25;
			var enableSsl = bool.TryParse(enableSslValue, out var parsedSsl) && parsedSsl;

			using var client = new SmtpClient(smtpHost, port)
			{
				EnableSsl = enableSsl
			};

			if (!string.IsNullOrWhiteSpace(smtpUser))
			{
				client.Credentials = new NetworkCredential(smtpUser, smtpPassword);
			}

			using var message = new MailMessage
			{
				From = new MailAddress(fromAddress),
				Subject = subject,
				Body = body,
				IsBodyHtml = false,
				BodyEncoding = Encoding.UTF8,
				SubjectEncoding = Encoding.UTF8
			};

			foreach (var recipient in recipients)
			{
				message.To.Add(recipient);
			}

			await client.SendMailAsync(message);
			_logger.LogInformation("Payment confirmation email sent. OrderId={OrderId}, Recipients={Recipients}", orderId, recipients.Count);
		}

		private static string BuildPaymentConfirmationBody(string? competitionName, IEnumerable<Models.Participant> participants)
		{
			var sb = new StringBuilder();
			sb.AppendLine("Здравствуйте!");
			sb.AppendLine();
			sb.AppendLine("Оплата прошла успешно.");

			if (!string.IsNullOrWhiteSpace(competitionName))
			{
				sb.AppendLine($"Соревнование: {competitionName}.");
			}

			sb.AppendLine("Дисциплины, на которые вы зарегистрированы:");
			sb.AppendLine();

			foreach (var participant in participants.OrderBy(p => p.LastName).ThenBy(p => p.FirstName))
			{
				var fullName = string.Join(' ', new[] { participant.LastName, participant.FirstName, participant.MiddleName }
					.Where(value => !string.IsNullOrWhiteSpace(value)));

				sb.AppendLine($"- {fullName}:");

				if (participant.Disciplines.Count == 0)
				{
					sb.AppendLine("  (дисциплины не указаны)");
					continue;
				}

				foreach (var discipline in participant.Disciplines)
				{
					var disciplineLine = $"{discipline.Name} {discipline.Distance}".Trim();
					sb.AppendLine($"  • {disciplineLine}");
				}
			}

			sb.AppendLine();
			sb.AppendLine("Спасибо за регистрацию!");

			return sb.ToString();
		}
	}
}
