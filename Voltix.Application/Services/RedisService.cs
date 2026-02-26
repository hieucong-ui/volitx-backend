using Voltix.Application.IServices;
using StackExchange.Redis;

namespace Voltix.Application.Services
{
    public class RedisService : IRedisService
    {
        private readonly IConnectionMultiplexer _redis;
        public RedisService(IConnectionMultiplexer redis)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        }

        public async Task<string?> RetrieveString(string key)
        {
            var cache = _redis.GetDatabase();
            var result = await cache.StringGetAsync(key);

            return result;
        }

        public async Task<bool> StoreKeyAsync(string key, string value, TimeSpan? expiry = null)
        {
            var cache = _redis.GetDatabase();
            var result = await cache.StringSetAsync(key, value, expiry);

            return result;
        }
    }
}
