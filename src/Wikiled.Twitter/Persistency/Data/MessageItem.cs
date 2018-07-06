using System;

namespace Wikiled.Twitter.Persistency.Data
{
    public class MessageItem
    {
        public MessageItem(UserItem user, TweetData data)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            Data = data ?? throw new ArgumentNullException(nameof(data));
            
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
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return data.Coordinates.GetDistanceTo(Coordinates);
        }
    }
}
