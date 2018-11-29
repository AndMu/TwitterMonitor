using System;
using System.Collections.Generic;
using System.Text;

namespace Wikiled.Twitter.Discovery
{
    public class DiscoveryRequest
    {
        public DiscoveryRequest(string[] topics)
        {
            Topics = topics;
            Enrichment = new string[] { };
        }

        public DiscoveryRequest(string[] topics, string[] enrichment)
        {
            Topics = topics ?? throw new ArgumentNullException(nameof(topics));
            Enrichment = enrichment ?? throw new ArgumentNullException(nameof(enrichment));
        }

        public string[] Topics { get; }

        public string[] Enrichment { get; }
    }
}
