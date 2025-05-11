using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;
using VlasikhaPlavanieWebsite.Application.Interfaces;
using VlasikhaPlavanieWebsite.ViewModels;

namespace VlasikhaPlavanieWebsite.Infrastructure.Services.Registration
{

	public class RedisRegistrationCache : IRegistrationCache
	{
		private readonly IDistributedCache _cache;
		private readonly ILogger<RedisRegistrationCache> _logger;

        public RedisRegistrationCache(IDistributedCache cache, ILogger<RedisRegistrationCache> logger)
        {
            _cache = cache;
			_logger = logger;
        }

        public async Task<string> CacheAsync(RegistrationViewModel viewModel)
		{
			_logger.LogInformation("Attempting to submit registration.");

			var orderId = Guid.NewGuid().ToString();
			_logger.LogInformation("Generated OrderId: {OrderId} for registration.", orderId);

			var cacheOptions = new DistributedCacheEntryOptions()
				.SetAbsoluteExpiration(TimeSpan.FromHours(3));

			const int maxRetries = 10;
			for (int attempt = 0; attempt < maxRetries; attempt++)
			{
				try
				{
					await _cache.SetStringAsync(orderId, JsonSerializer.Serialize(viewModel), cacheOptions);
					_logger.LogInformation("Registration data cached with OrderId: {OrderId}.", orderId);

					return orderId;
				}
				catch (RedisException ex)
				{
					_logger.LogWarning(ex, "Ошибка при попытке записи в Redis, попытка {Attempt} из {MaxRetries}", attempt + 1, maxRetries);
					if (attempt == maxRetries - 1)
					{
						throw new InvalidOperationException($"Не удалось сохранить данные регистрации в Redis после {maxRetries} попыток.", ex);
					}
					await Task.Delay(5000);
				}
			}

			throw new InvalidOperationException("Unexpected error in CacheAsync.");
		}
	}
}