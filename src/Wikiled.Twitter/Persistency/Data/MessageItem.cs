using System;
using Wikiled.Common.Arguments;
using Wikiled.Twitter.Discovery;

namespace Wikiled.Twitter.Persistency.Data
{
    public class MessageItem
    {
        public MessageItem(UserItem user, TweetData data)
        {
            Guard.NotNull(() => user, user);
            Guard.NotNull(() => data, data);
            User = user;
            Data = data;
            
            Coordinates = new GeoCoordinate(data.Latitude, data.Longitude);
            DateTime = new DateTime(data.Tick);
        }
        public TweetData Data { get; }

        public UserItem User { get; }

        public MessageItem Retweet { get; set; }

        public GeoCoordinate Coordinates { get; }

        public DateTime DateTime { get; }

        public double CalculateDistance(MessageItem data)
        {
            Guard.NotNull(() => data, data);
            return data.Coordinates.GetDistanceTo(Coordinates);
        }
    }
}
