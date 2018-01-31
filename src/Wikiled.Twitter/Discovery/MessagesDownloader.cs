using System;
using System.Reactive.Linq;
using Tweetinvi;
using Tweetinvi.Models;

namespace Wikiled.Twitter.Discovery
{
    public class MessagesDownloader : IMessagesDownloader
    {
        public IObservable<ITweet> Download(IObservable<long> ids)
        {
            return ids.Select(Tweet.GetTweet);
        }
    }
}
