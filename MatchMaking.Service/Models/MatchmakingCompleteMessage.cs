namespace MatchMaking.Service.Models
{
    public record MatchmakingCompleteMessage(string MatchId, IReadOnlyList<string> UserIds);
}
