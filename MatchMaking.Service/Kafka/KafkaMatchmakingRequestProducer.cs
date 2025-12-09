using Confluent.Kafka;
using MatchMaking.Service.Models;
using System.Text.Json;

namespace MatchMaking.Service.Kafka
{

    public class KafkaMatchmakingRequestProducer : IMatchmakingRequestProducer
    {
        private readonly IProducer<string, string> _producer;
        private readonly string _topic;

        public KafkaMatchmakingRequestProducer(IConfiguration config)
        {
            var conf = new ProducerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"] ?? "localhost:9092"
            };

            _topic = config["Kafka:RequestTopic"] ?? "matchmaking.request";
            _producer = new ProducerBuilder<string, string>(conf).Build();
        }

        public async Task SendAsync(string userId)
        {
            var payload = JsonSerializer.Serialize(new MatchmakingRequestMessage(userId));

            await _producer.ProduceAsync(_topic, new Message<string, string>
            {
                Key = userId,
                Value = payload
            });
        }

        public void Dispose()
        {
            _producer.Flush();
            _producer.Dispose();
        }
    }
}
