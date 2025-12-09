
using StackExchange.Redis;

namespace MatchMaking.Service.Redis
{
    public class RedisRateLimiter : IRateLimiter
    {
        private readonly IDatabase _db;
        private readonly TimeSpan _interval = TimeSpan.FromMilliseconds(100);

        public RedisRateLimiter(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task<bool> IsAllowedAsync(string userId)
        {
            var key = $"ratelimit:{userId}";
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var script = @"
                local last = redis.call('GET', KEYS[1])
                local now = tonumber(ARGV[1])
                local interval = tonumber(ARGV[2])

                if not last then
                  redis.call('SET', KEYS[1], now)
                  return 1
                end

                last = tonumber(last)

                if (now - last) >= interval then
                  redis.call('SET', KEYS[1], now)
                  return 1
                end

                return 0
                ";

            var result = (long)await _db.ScriptEvaluateAsync(
                script,
                new RedisKey[] { key },
                new RedisValue[] { now, (long)_interval.TotalMilliseconds });

            return result == 1;
        }
    }
}
