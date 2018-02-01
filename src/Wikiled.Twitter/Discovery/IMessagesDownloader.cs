using System.Collections.Generic;
using Tweetinvi.Models;

namespace Wikiled.Twitter.Discovery
{
    public interface IMessagesDownloader
    {
        IEnumerable<ITweet> Download(long[] ids);
    }
}