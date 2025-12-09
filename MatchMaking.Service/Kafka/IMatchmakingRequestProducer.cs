namespace MatchMaking.Service.Kafka
{
    public interface IMatchmakingRequestProducer
    {
        Task SendAsync(string userId);
    }
}
