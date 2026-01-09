using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VlasikhaPlavanieWebsite.Application.Interfaces;

namespace VlasikhaPlavanieWebsite.Infrastructure.Services.Email
{
	public class EmailQueueHostedService : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly EmailQueue _queue;
		private readonly ILogger<EmailQueueHostedService> _logger;

		public EmailQueueHostedService(IServiceProvider serviceProvider, EmailQueue queue, ILogger<EmailQueueHostedService> logger)
		{
			_serviceProvider = serviceProvider;
			_queue = queue;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await foreach (var orderId in _queue.Reader.ReadAllAsync(stoppingToken))
			{
				try
				{
					using var scope = _serviceProvider.CreateScope();
					var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
					await emailService.SendPaymentConfirmationAsync(orderId);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Failed to send queued payment confirmation email. OrderId={OrderId}", orderId);
				}
			}
		}
	}
}
