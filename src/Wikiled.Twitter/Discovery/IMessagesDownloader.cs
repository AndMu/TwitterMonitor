using System;
using Tweetinvi.Models;

namespace Wikiled.Twitter.Discovery
{
    public interface IMessagesDownloader
    {
        IObservable<ITweet> Download(IObservable<long> ids);
    }
}