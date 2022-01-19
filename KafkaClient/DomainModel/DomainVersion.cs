using System;

namespace KafkaMessagesExtractor.DomainModel
{
    public class DomainVersionModel
    {
        public Guid AccessId { get; set; }
        public int Version { get; set; }
        public DateTime Timestamp { get; set; }
    }
}