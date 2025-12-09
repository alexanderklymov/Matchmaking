using Confluent.Kafka;
using MatchMaking.Worker.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MatchMaking.Worker.Kafka;

public class MatchmakingWorker : BackgroundService
{
    private readonly ILogger<MatchmakingWorker> _logger;
    private readonly IConfiguration _config;

    public MatchmakingWorker(ILogger<MatchmakingWorker> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => RunWorker(stoppingToken), stoppingToken);
    }

    private void RunWorker(CancellationToken ct)
    {
        var playersPerMatch = int.Parse(_config["Matchmaking:PlayersPerMatch"] ?? "3");

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _config["Kafka:BootstrapServers"] ?? "kafka:9092",
            GroupId = _config["Kafka:ConsumerGroup"] ?? "matchmaking-worker",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _config["Kafka:BootstrapServers"] ?? "kafka:9092"
        };

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        using var producer = new ProducerBuilder<string, string>(producerConfig).Build();

        var requestTopic = _config["Kafka:RequestTopic"] ?? "matchmaking.request";
        var completeTopic = _config["Kafka:CompleteTopic"] ?? "matchmaking.complete";

        consumer.Subscribe(requestTopic);

        var buffer = new List<string>();

        _logger.LogInformation("Worker started. Players per match = {Count}", playersPerMatch);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var cr = consumer.Consume(ct);
                var request = JsonSerializer.Deserialize<MatchmakingRequestMessage>(cr.Message.Value);
                if (request == null) continue;

                buffer.Add(request.UserId);
                _logger.LogInformation("Received request from {UserId}. Buffer size = {Size}", request.UserId, buffer.Count);

                if (buffer.Count >= playersPerMatch)
                {
                    var users = buffer.Take(playersPerMatch).ToList();
                    buffer.RemoveRange(0, playersPerMatch);

                    var matchId = Guid.NewGuid().ToString();

                    var complete = new MatchmakingCompleteMessage(matchId, users);
                    var json = JsonSerializer.Serialize(complete);

                    producer.Produce(completeTopic, new Message<string, string>
                    {
                        Key = matchId,
                        Value = json
                    });

                    _logger.LogInformation("Created match {MatchId} for users: {Users}", matchId, string.Join(",", users));
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing messages");
            }
        }
    }
}
