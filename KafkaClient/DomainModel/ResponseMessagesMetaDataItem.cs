namespace KafkaMessagesExtractor.DomainModel
{
    public class DomainMessagesMetaDataItem
    {
        public int Partition { get; set; }
        public int LastOffset { get; set; }
    }
}