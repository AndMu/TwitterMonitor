using System;
using Tweetinvi.Models;
using Wikiled.Common.Utilities.Config;

namespace Wikiled.Twitter.Security
{
    public class EnvironmentAuthentication : IAuthentication
    {
        readonly IApplicationConfiguration config;

        public EnvironmentAuthentication(IApplicationConfiguration config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public ITwitterCredentials Authenticate()
        {
            return new TwitterCredentials(
                config.GetEnvironmentVariable("TW_CONSUMER_KEY")?.Trim(),
                config.GetEnvironmentVariable("TW_CONSUMER_SECRET")?.Trim(),
                config.GetEnvironmentVariable("TW_APP_KEY")?.Trim(),
                config.GetEnvironmentVariable("TW_APP_SECRET")?.Trim());
        }
    }
}