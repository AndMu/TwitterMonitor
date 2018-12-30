using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MoreLinq.Extensions;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using Wikiled.Common.Extensions;

namespace Wikiled.Twitter.Discovery
{
    public class MessageDiscovery : IMessageDiscovery
    {
        private readonly ILogger<MessageDiscovery> log;

        private readonly DiscoveryRequest request;

        private readonly HashSet<long> processed = new HashSet<long>();

        public MessageDiscovery(ILogger<MessageDiscovery> log, DiscoveryRequest request)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this.request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public LanguageFilter Language { get; } = LanguageFilter.English;

        public int BatchSize { get; set; } = 1;

        public long[] Processed => processed.ToArray();

        public void AddProcessed(long[] ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            foreach (var id in ids)
            {
                processed.Add(id);
            }
        }

        public IEnumerable<(ITweet Message, string Topic)> Process()
        {
            if (request.Enrichment.Length > 0)
            {
                foreach (var batch in request.Enrichment.Batch(BatchSize))
                {
                    var item = batch.AccumulateItems(" OR ");
                    var result = ProcessEnrichment(item);
                    foreach (var resultItem in result)
                    {
                        yield return resultItem;
                    }
                }
            }
            else
            {
                var result = ProcessEnrichment(string.Empty);
                foreach (var resultItem in result)
                {
                    yield return resultItem;
                }
            }
        }

        private IEnumerable<(ITweet Message, string Topic)> ProcessEnrichment(string enrichmentItem)
        {
            foreach (var topic in request.Topics)
            {
                var total = 0;
                var lastSearch = DateTime.Now;
                do
                {
                    total = 0;
                    var searchParameter = GetParameter(topic, enrichmentItem, lastSearch);
                    var tweets = Search.SearchTweets(searchParameter) ?? Search.SearchTweets(searchParameter);
                    if (tweets == null)
                    {
                        log.LogDebug("Not Found for [{0}]", searchParameter.SearchQuery);
                        yield break;
                    }

                    log.LogDebug("Retrieved for [{0}]", searchParameter.SearchQuery);
                    foreach (var tweet in tweets)
                    {
                        total++;
                        if (tweet.CreatedAt < lastSearch)
                        {
                            lastSearch = tweet.CreatedAt;
                        }

                        if (!processed.Contains(tweet.Id))
                        {
                            processed.Add(tweet.Id);
                            yield return (tweet, topic);
                        }
                    }
                }
                while (total > 0);
            }
        }

        private SearchTweetsParameters GetParameter(string topic, string enrichmentItem, DateTime until)
        {
            var searchParameter = new SearchTweetsParameters($"\"{topic}\" {enrichmentItem} -filter:retweets");
            searchParameter.Lang = Language;
            searchParameter.Until = until;
            searchParameter.MaximumNumberOfResults = 100;
            return searchParameter;

        }
    }
}
