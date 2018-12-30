using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Wikiled.Twitter.Persistency.Data
{
    public class UserItem
    {
        private readonly ConcurrentDictionary<long, MessageItem> messages = new ConcurrentDictionary<long, MessageItem>();

        public UserItem(TweetUser user)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public double AverageDistance
        {
            get
            {
                double distance = 0;
                var copy = Messages.OrderBy(item => item.Data.Tick).ToArray();
                if (copy.Length == 0)
                {
                    return 0;
                }

                for (var i = 1; i < copy.Length; i++)
                {
                    distance += copy[i].CalculateDistance(copy[i - 1]);
                }

                return distance / copy.Length;
            }
        }

        public bool HasOutOfWorkMessage
        {
            get
            {
                return Messages.Any(item => item.DateTime.Hour > 20 || item.DateTime.Hour < 8);
            }
        }

        public bool HasWorkMessage
        {
            get
            {
                return Messages.Any(item => item.DateTime.Hour < 20 && item.DateTime.Hour > 8);
            }
        }

        public MessageItem[] Messages => messages.Values.ToArray();

        public TweetUser User { get; }

        public void Add(MessageItem message)
        {
            messages[message.Data.Tick] = message;
        }
    }
}
