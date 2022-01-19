namespace KafkaMessagesExtractor.DomainModel
{
    public class DomainMessage
    {
        public DomainData Data { get; set; }
        public DomainMetaData MetaData { get; set; }
    }
}