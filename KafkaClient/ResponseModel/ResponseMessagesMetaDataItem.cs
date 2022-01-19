namespace KafkaMessagesExtractor.ResponseModel
{
    public class ResponseMessagesMetaDataItem
    {
        public int Partition { get; set; }
        public int LastOffset { get; set; }
    }
}