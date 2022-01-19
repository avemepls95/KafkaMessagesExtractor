namespace KafkaMessagesExtractor
{
    public sealed class SearchParameters
    {
        public string Url { get; set; }
        public string TopicName { get; set; }
        public int Partition { get; set; }
        public int Offset { get; set; }
        public int Count { get; set; }
    }
}