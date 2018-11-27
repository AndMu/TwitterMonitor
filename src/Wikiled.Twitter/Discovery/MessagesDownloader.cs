using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MoreLinq;
using NLog;
using Tweetinvi;
using Tweetinvi.Models;

namespace Wikiled.Twitter.Discovery
{
    public class MessagesDownloader : IMessagesDownloader
    {
        private readonly ILogger<MessagesDownloader> log;

        public MessagesDownloader(ILogger<MessagesDownloader> log)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public IEnumerable<ITweet> Download(long[] ids)
        {
            log.LogDebug("Downloading {0} messages", ids.Length);
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
