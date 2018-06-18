using System;
using Tweetinvi.Models;

namespace Wikiled.Twitter.Security
{
    public class BasicAuthentication : IAuthentication
    {
        private readonly ITwitterCredentials credentials;

        public BasicAuthentication(ITwitterCredentials credentials)
        {
            this.credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
        }

        public ITwitterCredentials Authenticate(ITwitterCredentials applicationCredentials)
        {
            return credentials;
        }
    }
}
