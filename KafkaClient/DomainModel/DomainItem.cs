using System;

namespace KafkaMessagesExtractor.DomainModel
{
    public class DomainItem
    {
        public int Partition { get; set; }
        public int Offset { get; set; }
        public DomainMessage Message { get; set; }
        public Guid Key { get; set; }
        public DateTime Timestamp { get; set; }
    }
}