using StackExchange.Redis;

namespace Voltix.API.Extentions
{
    public static class RedisServiceExtensions
    {
        public static WebApplicationBuilder AddRedisCacheService(this WebApplicationBuilder builder)
        {

            string? connectionString = builder.Configuration["Redis__ConnectionString"]
                ?? builder.Configuration["Redis:ConnectionString"]
                ?? throw new InvalidOperationException("Redis connection string is not configured.");

            builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(connectionString));
            return builder;
        }
    }
}
