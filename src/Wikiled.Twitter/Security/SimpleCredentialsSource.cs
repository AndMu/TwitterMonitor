using System;
using Tweetinvi.Models;

namespace Wikiled.Twitter.Security
{
    public class SimpleCredentialsSource : ICredentialsSource
    {
        private readonly ITwitterCredentials credentials;

        public SimpleCredentialsSource(ITwitterCredentials credentials)
        {
            this.credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
        }

        public ITwitterCredentials Resolve()
        {
            return credentials;
        }
    }
}
