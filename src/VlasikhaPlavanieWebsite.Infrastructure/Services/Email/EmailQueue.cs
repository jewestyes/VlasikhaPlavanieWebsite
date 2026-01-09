using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using VlasikhaPlavanieWebsite.Application.Interfaces;

namespace VlasikhaPlavanieWebsite.Infrastructure.Services.Email
{
	public class EmailQueue : IEmailQueue
	{
		private readonly Channel<int> _queue = Channel.CreateUnbounded<int>();
		private readonly ILogger<EmailQueue> _logger;

		public EmailQueue(ILogger<EmailQueue> logger)
		{
			_logger = logger;
		}

		public void EnqueuePaymentConfirmation(int orderId)
		{
			if (!_queue.Writer.TryWrite(orderId))
			{
				_logger.LogWarning("Failed to enqueue payment confirmation email. OrderId={OrderId}", orderId);
			}
		}

		public ChannelReader<int> Reader => _queue.Reader;
	}
}
