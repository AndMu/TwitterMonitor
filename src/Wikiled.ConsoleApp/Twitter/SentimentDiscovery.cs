using Wikiled.Arff.Logic;
using Wikiled.Twitter.Discovery;

namespace Wikiled.ConsoleApp.Twitter
{
    public class SentimentDiscovery
    {
        public SentimentDiscovery(PositivityType type, IMessageDiscovery discovery)
        {
            Type = type;
            Discovery = discovery;
        }

        public IMessageDiscovery Discovery { get; }

        public PositivityType Type { get; }
    }
}
