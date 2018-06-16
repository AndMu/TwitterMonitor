using Tweetinvi.Models;

namespace Wikiled.Twitter.Security
{
    public interface IAuthentication
    {
        ITwitterCredentials Authenticate(ITwitterCredentials applicationCredentials);

    }
}