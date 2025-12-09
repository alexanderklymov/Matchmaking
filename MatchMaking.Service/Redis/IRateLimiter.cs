namespace MatchMaking.Service.Redis
{
    public interface IRateLimiter
    {
        Task<bool> IsAllowedAsync(string userId);
    }
}
