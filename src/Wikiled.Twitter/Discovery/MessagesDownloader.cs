using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using NLog;
using Tweetinvi;
using Tweetinvi.Models;

namespace Wikiled.Twitter.Discovery
{
    public class MessagesDownloader : IMessagesDownloader
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public IEnumerable<ITweet> Download(long[] ids)
        {
            log.Debug("Downloading {0} messages", ids.Length);
            foreach (var batch in ids.Batch(100))
            {
                var result = Tweet.GetTweets(batch.ToArray());
                
                foreach (var tweet in result)
                {
                    yield return tweet;
                }
            }
        }
    }
}
