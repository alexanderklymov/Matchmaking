using MatchMaking.Service.Models;

namespace MatchMaking.Service.Redis
{
    public interface IMatchRepository
    {
        Task StoreMatchAsync(MatchInfo match);
        Task SetUserLastMatchAsync(string userId, string matchId);
        Task<MatchInfo?> GetLastMatchForUserAsync(string userId);
    }
}
