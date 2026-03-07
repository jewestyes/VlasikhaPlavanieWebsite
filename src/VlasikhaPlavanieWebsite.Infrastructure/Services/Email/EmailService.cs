using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VlasikhaPlavanieWebsite.Application.Interfaces;
using VlasikhaPlavanieWebsite.Data;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Threading;
using System.IO;

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

			var participantsByEmail = order.Participants
				.Where(p => !string.IsNullOrWhiteSpace(p.Email))
				.Select(p => new { Participant = p, Email = p.Email.Trim() })
				.GroupBy(x => x.Email, StringComparer.OrdinalIgnoreCase)
				.ToList();

			if (participantsByEmail.Count == 0)
			{
				_logger.LogWarning("Payment confirmation email skipped because no recipient emails found. OrderId={OrderId}", orderId);
				return;
			}

			var from = _configuration["Email:From"];
			var host = _configuration["Email:Host"];
			var portValue = _configuration["Email:Port"];
			var username = _configuration["Email:Username"];
			var password = _configuration["Email:Password"];
			var useSsl = _configuration["Email:UseSsl"];
			var useStartTls = _configuration["Email:UseStartTls"];

			if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(host))
			{
				_logger.LogWarning("Email settings are missing; skipping confirmation email. OrderId={OrderId}", orderId);
				return;
			}

			var subject = $"Оплата подтверждена • {order.Competition?.Name ?? "Регистрация"}";

			var port = int.TryParse(portValue, out var parsedPort) ? parsedPort : 25;
			var enableSsl = bool.TryParse(useSsl, out var parsedSsl) && parsedSsl;
			var startTls = bool.TryParse(useStartTls, out var parsedStartTls) && parsedStartTls;

			_logger.LogInformation("SMTP settings: Host={Host}, Port={Port}, EnableSsl={EnableSsl}, UsernameSet={HasUsername}", host, port, enableSsl, !string.IsNullOrWhiteSpace(username));

			// Use MailKit for robust TLS support (supports implicit SSL on 465)
			var secureOption = startTls ? SecureSocketOptions.StartTls : SecureSocketOptions.SslOnConnect;

			using var smtp = new MailKit.Net.Smtp.SmtpClient();

			try
			{
				var connectTask = smtp.ConnectAsync(host, port, secureOption, CancellationToken.None);
				await connectTask;

				if (!string.IsNullOrWhiteSpace(username))
				{
					await smtp.AuthenticateAsync(username, password, CancellationToken.None);
				}

				foreach (var group in participantsByEmail)
				{
					try
					{
						var recipient = group.Key;
						var participantList = group.Select(x => x.Participant).ToList();

						var textBody = BuildPaymentConfirmationBody(order.Competition?.Name, participantList);

						// Load template from configured path
						var templatePathConfig = _configuration["Email:TemplatePath"];
						// Template should be copied to output (bin) and referenced relative to AppContext.BaseDirectory
						var templatePath = Path.Combine(AppContext.BaseDirectory ?? ".", templatePathConfig ?? "Templates/payment_confirmation.html");

						var templateExists = File.Exists(templatePath);
						if (!templateExists)
						{
							_logger.LogWarning("Email template not found at path {TemplatePath}. Sending text-only email. OrderId={OrderId}", templatePath, orderId);
						}

						string? htmlBody = null;
						if (templateExists)
						{
							try
							{
								var templateContent = File.ReadAllText(templatePath);
								var listHtml = BuildParticipantsListHtml(participantList);
								var safeTitle = WebUtility.HtmlEncode(order.Competition?.Name ?? "Регистрация");
								htmlBody = templateContent.Replace("{{Title}}", safeTitle).Replace("{{List}}", listHtml);
							}
							catch (Exception ex)
							{
								_logger.LogError(ex, "Failed to read or process email template at {TemplatePath}. Sending text-only. OrderId={OrderId}", templatePath, orderId);
								htmlBody = null;
							}
						}

						var message = new MimeMessage();
						var senderAddress = MailboxAddress.Parse(!string.IsNullOrWhiteSpace(username) ? username : from);
						message.Sender = senderAddress;
						message.From.Add(new MailboxAddress("Аква Олимп", from));


						// Add default CC addresses from configuration (comma or semicolon separated)
						var defaultCc = _configuration["Email:DefaultBcc"];
						if (!string.IsNullOrWhiteSpace(defaultCc))
						{
							var addrs = defaultCc.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries)
								.Select(a => a.Trim())
								.Where(a => !string.IsNullOrWhiteSpace(a));
							foreach (var bcc in addrs)
							{
								try
								{
									message.Bcc.Add(MailboxAddress.Parse(bcc));
								}
								catch (Exception ex)
								{
									_logger.LogWarning(ex, "Invalid BCC address skipped: {Address}", bcc);
								}
							}
						}

						message.To.Add(MailboxAddress.Parse(recipient));
						message.Subject = subject;

						var builder = new BodyBuilder
						{
							TextBody = textBody
						};

						if (!string.IsNullOrWhiteSpace(htmlBody))
						{
							builder.HtmlBody = htmlBody;
						}

						message.Body = builder.ToMessageBody();

						_logger.LogInformation("Sending email to {Recipient} for OrderId={OrderId}", recipient, orderId);

						await smtp.SendAsync(message, CancellationToken.None);

						_logger.LogInformation("Payment confirmation email sent. OrderId={OrderId}, Recipient={Recipient}, Participants={Participants}", orderId, recipient, participantList.Count);
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Failed to send payment confirmation email to recipient. OrderId={OrderId}, Recipient={Recipient}", orderId, group.Key);
					}
				}

				await smtp.DisconnectAsync(true, CancellationToken.None);
			}
			catch (MailKit.ServiceNotConnectedException ex)
			{
				_logger.LogError(ex, "SMTP connect failed. OrderId={OrderId}, Host={Host}, Port={Port}", orderId, host, port);
			}
			catch (MailKit.ServiceNotAuthenticatedException ex)
			{
				_logger.LogError(ex, "SMTP authentication failed. OrderId={OrderId}, Host={Host}", orderId, host);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected error while sending payment confirmation email. OrderId={OrderId}", orderId);
				throw;
			}
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

		// Replace BuildParticipantsListHtml to use email-safe inline styles matching template to prevent layout shifts
		private static string BuildParticipantsListHtml(IEnumerable<Models.Participant> participants)
		{
			var list = new StringBuilder();

			foreach (var participant in participants.OrderBy(p => p.LastName).ThenBy(p => p.FirstName))
			{
				var fullName = string.Join(' ', new[] { participant.LastName, participant.FirstName, participant.MiddleName }
					.Where(value => !string.IsNullOrWhiteSpace(value)));

				var safeName = WebUtility.HtmlEncode(fullName);
				var birth = participant.BirthDate.ToString("yyyy-MM-dd");
				var rank = WebUtility.HtmlEncode(participant.Rank ?? string.Empty);

				list.AppendLine("<tr>");
				list.AppendLine("  <td style=\"padding:16px 20px; border-top:1px solid #e6e8ee;\">\n");
				list.AppendLine($"    <div style=\"font:600 14px/20px -apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Arial,sans-serif; color:#0b1220;\">{safeName}</div>\n");
				list.AppendLine($"    <div style=\"margin-top:6px; font:400 13px/18px -apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Arial,sans-serif; color:#667085;\">Дата рождения: {birth} • КМС: {rank}</div>\n");

				if (participant.Disciplines.Count == 0)
				{
					list.AppendLine("    <div style=\"margin-top:6px; font:400 13px/18px -apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Arial,sans-serif; color:#667085;\">Дисциплины не указаны</div>\n");
				}
				else
				{
					list.AppendLine("    <div style=\"margin-top:8px;\">\n");
					list.AppendLine("      <table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"border-collapse:collapse; width:100%;\">\n");

					foreach (var d in participant.Disciplines)
					{
						var disciplineLine = $"{d.Name} {d.Distance} {d.EntryTime}".Trim();
						var safeDiscipline = WebUtility.HtmlEncode(disciplineLine);

						list.AppendLine("        <tr>");
						list.AppendLine("          <td style=\"width:18px; vertical-align:top; padding-top:2px;\">\n");
						list.AppendLine("            <div style=\"width:8px; height:8px; border-radius:999px; background:#2563eb;\"></div>\n");
						list.AppendLine("          </td>\n");
						list.AppendLine($"          <td style=\"padding:0 0 8px 0; font:400 13px/18px -apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Arial,sans-serif; color:#0b1220;\">{safeDiscipline}</td>\n");
						list.AppendLine("        </tr>\n");
					}

					list.AppendLine("      </table>\n");
					list.AppendLine("    </div>\n");
				}

				list.AppendLine("  </td>\n");
				list.AppendLine("</tr>\n");
			}

			return list.ToString();
		}
	}
}
