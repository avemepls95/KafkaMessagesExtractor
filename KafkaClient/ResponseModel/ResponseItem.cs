using System;

namespace KafkaMessagesExtractor.ResponseModel
{
    public class ResponseItem
    {
        public ResponseItem()
        {
        }

        public int Partition { get; set; }
        public int Offset { get; set; }
        public dynamic Message { get; set; }
        public Guid Key { get; set; }
        public DateTime Timestamp { get; set; }
    }
}