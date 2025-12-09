using Confluent.Kafka;
using MatchMaking.Service.Models;
using MatchMaking.Service.Redis;
using System.Text.Json;

namespace MatchMaking.Service.Kafka;

public class MatchmakingCompleteConsumer : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly IMatchRepository _repo;
    private readonly ILogger<MatchmakingCompleteConsumer> _logger;

    public MatchmakingCompleteConsumer(
        IConfiguration config,
        IMatchRepository repo,
        ILogger<MatchmakingCompleteConsumer> logger)
    {
        _config = config;
        _repo = repo;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => Run(stoppingToken), stoppingToken);
    }

    private void Run(CancellationToken ct)
    {
        var conf = new ConsumerConfig
        {
            BootstrapServers = _config["Kafka:BootstrapServers"] ?? "kafka:9092",
            GroupId = _config["Kafka:ServiceConsumerGroup"] ?? "matchmaking-service",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        var topic = _config["Kafka:CompleteTopic"] ?? "matchmaking.complete";

        using var consumer = new ConsumerBuilder<string, string>(conf).Build();
        consumer.Subscribe(topic);

        _logger.LogInformation("MatchmakingCompleteConsumer started, listening to {Topic}", topic);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var cr = consumer.Consume(ct);
                var msg = JsonSerializer.Deserialize<MatchmakingCompleteMessage>(cr.Message.Value);
                if (msg == null) continue;

                var match = new MatchInfo(msg.MatchId, msg.UserIds);

                _repo.StoreMatchAsync(match).Wait();
                foreach (var user in msg.UserIds)
                    _repo.SetUserLastMatchAsync(user, msg.MatchId).Wait();

                _logger.LogInformation("Stored match {MatchId} with {Count} users", msg.MatchId, msg.UserIds.Count);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MatchmakingCompleteConsumer");
            }
        }
    }
}
