using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace KafkaMessagesExtractor
{
    public class OutputMessagesContainer : IEnumerable
    {
        private readonly List<string> _messages = new List<string>();

        public void Add(string message)
        {
            _messages.Add(message);
        }

        public bool Any()
        {
            return _messages.Any();
        }

        public IEnumerator GetEnumerator()
        {
            return _messages.GetEnumerator();
        }
    }
}