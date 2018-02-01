using System.Collections.Generic;
using Tweetinvi.Models;

namespace Wikiled.Twitter.Discovery
{
    public interface IMessageDiscovery
    {
        LanguageFilter Language { get; }

        int BatchSize { get; set; }

        long[] Processed { get; }

        void AddProcessed(long[] ids);

        IEnumerable<(ITweet Message, string Topic)> Process();
    }
}