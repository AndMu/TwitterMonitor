using Tweetinvi.Models;

namespace Wikiled.Twitter.Security
{
    public interface ICredentialsSource
    {
        ITwitterCredentials Resolve();
    }
}