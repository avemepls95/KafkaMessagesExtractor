using System;

namespace KafkaMessagesExtractor.ResponseModel
{
    public class ResponseVersionModel
    {
        public Guid AccessId { get; set; }
        public int Version { get; set; }
        public DateTime Timestamp { get; set; }
    }
}