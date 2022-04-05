using System;
using Newtonsoft.Json;

namespace KafkaMessagesExtractor.DomainModel
{
    public class DomainItem
    {
        public int Partition { get; set; }
        public int Offset { get; set; }
        public DomainMessage Message { get; set; }
        public Guid Key { get; set; }
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this)
                .Replace("\\\"", "\"")
                .Replace("\"{", "{")
                .Replace("}\"", "}");
        }
    }
}