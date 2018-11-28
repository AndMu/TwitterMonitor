using System;
using Microsoft.Extensions.Logging;
using Tweetinvi.Models;
using Wikiled.Common.Utilities.Config;

namespace Wikiled.Twitter.Security
{
    public class EnvironmentCredentialsSource : ICredentialsSource
    {
        private readonly ILogger<EnvironmentAuthentication> log;

        private readonly IApplicationConfiguration config;

        public EnvironmentCredentialsSource(ILogger<EnvironmentAuthentication> log, IApplicationConfiguration config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public ITwitterCredentials Resolve()
        {
            var credentials = new TwitterCredentials(
                config.GetEnvironmentVariable("TW_CONSUMER_KEY")?.Trim(),
                config.GetEnvironmentVariable("TW_CONSUMER_SECRET")?.Trim());
            if (string.IsNullOrEmpty(credentials.ConsumerKey) ||
                string.IsNullOrEmpty(credentials.ConsumerSecret))
            {
                log.LogError("Failed to read credentials. [{0}] [{1}]",
                             credentials.ConsumerKey,
                             credentials.ConsumerSecret);
                throw new Exception("Failed to read credentials");
            }

            return credentials;
        }
    }
}
