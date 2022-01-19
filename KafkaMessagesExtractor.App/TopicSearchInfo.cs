namespace KafkaMessagesExtractor.App
{
    public class TopicSearchInfo
    {
        public int Partition { get; set; }
        public int CurrentLastOffset { get; set; }
    }
}