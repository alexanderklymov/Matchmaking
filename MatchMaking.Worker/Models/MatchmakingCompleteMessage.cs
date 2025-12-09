namespace MatchMaking.Worker.Models
{
    public record MatchmakingCompleteMessage(string MatchId, IReadOnlyList<string> UserIds);
}
