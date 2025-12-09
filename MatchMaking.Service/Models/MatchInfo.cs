namespace MatchMaking.Service.Models
{
    public record MatchInfo(string MatchId, IReadOnlyList<string> UserIds);
}
