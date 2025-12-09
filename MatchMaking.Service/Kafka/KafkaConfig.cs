namespace MatchMaking.Service.Kafka
{
    public class KafkaConfig
    {
        public string BootstrapServers { get; set; }
        public string RequestTopic { get; set; }
        public string CompleteTopic { get; set; }
    }
}

