using System;
using Microsoft.Extensions.Logging;
using Tweetinvi.Models;
using Wikiled.Common.Utilities.Config;

namespace Wikiled.Twitter.Security
{
    public class EnvironmentAuthentication : IAuthentication
    {
        private readonly ILogger<EnvironmentAuthentication> log;

        private readonly IApplicationConfiguration config;

        public EnvironmentAuthentication(ILogger<EnvironmentAuthentication> log, IApplicationConfiguration config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public ITwitterCredentials Authenticate()
        {
            var credentials = new TwitterCredentials(
                config.GetEnvironmentVariable("TW_CONSUMER_KEY")?.Trim(),
                config.GetEnvironmentVariable("TW_CONSUMER_SECRET")?.Trim(),
                config.GetEnvironmentVariable("TW_APP_KEY")?.Trim(),
                config.GetEnvironmentVariable("TW_APP_SECRET")?.Trim());
            if (string.IsNullOrEmpty(credentials.AccessToken) ||
                string.IsNullOrEmpty(credentials.AccessTokenSecret) ||
                string.IsNullOrEmpty(credentials.ConsumerKey) ||
                string.IsNullOrEmpty(credentials.ConsumerSecret))
            {
                log.LogError("Failed to read credentials. [{0}] [{1}] [{2}] [{3}]",
                    credentials.AccessTokenSecret,
                    credentials.AccessToken,
                    credentials.ConsumerKey,
                    credentials.ConsumerSecret);
                throw new Exception("Failed to read credentials");
            }

            return credentials;
        }
    }
}