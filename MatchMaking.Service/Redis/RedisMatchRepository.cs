using MatchMaking.Service.Models;
using StackExchange.Redis;
using System.Text.Json;
using System.Threading.Tasks;


namespace MatchMaking.Service.Redis
{
    public class RedisMatchRepository : IMatchRepository
    {
        private readonly IDatabase _db;
        private static string MatchKey(string id) => $"match:{id}";
        private static string UserKey(string id) => $"user:lastMatch:{id}";

        public RedisMatchRepository(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task<MatchInfo?> GetLastMatchForUserAsync(string userId)
        {
            var matchId = await _db.StringGetAsync(UserKey(userId));
            if (matchId.IsNullOrEmpty) return null;

            var json = await _db.StringGetAsync(MatchKey(matchId!));
            if (json.IsNullOrEmpty) return null;

            return JsonSerializer.Deserialize<MatchInfo>(json!);
        }

        public async Task SetUserLastMatchAsync(string userId, string matchId)
        {
            await _db.StringSetAsync(UserKey(userId), matchId);
        }

        public async Task StoreMatchAsync(MatchInfo match)
        {
            var json = JsonSerializer.Serialize(match);
            await _db.StringSetAsync(MatchKey(match.MatchId), json);
        }
    }
}
