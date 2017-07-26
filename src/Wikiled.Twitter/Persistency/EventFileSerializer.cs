using System;
using Tweetinvi.Models.DTO;

namespace Wikiled.Twitter.Persistency
{
    public class EventFileSerializer : IPersistency
    {
        public event EventHandler<ITweetDTO> Received;

        public void Save(ITweetDTO tweet)
        {
            Received?.Invoke(this, tweet);
        }
    }
}
