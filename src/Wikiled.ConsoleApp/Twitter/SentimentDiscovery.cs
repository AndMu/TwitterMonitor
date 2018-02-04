using Wikiled.Arff.Persistence;
using Wikiled.Twitter.Discovery;

namespace Wikiled.ConsoleApp.Twitter
{
    public class SentimentDiscovery
    {
        public SentimentDiscovery(PositivityType type, MessageDiscovery discovery)
        {
            Type = type;
            Discovery = discovery;
        }

        public MessageDiscovery Discovery { get; }

        public PositivityType Type { get; }
    }
}
