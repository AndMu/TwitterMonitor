using Tweetinvi;
using Tweetinvi.Models;

namespace Wikiled.Twitter.Security
{
    public class Authentication
    {
        private Authentication()
        {

            IphoneTwitterCredentials = Auth.SetApplicationOnlyCredentials("IQKbtAYlXLripLGPWd0HUA", "GgDYlkSvaPxGxC4X8liwpUoqKwwr3lCADbz8A7ADU");
        }

        public ITwitterCredentials IphoneTwitterCredentials { get; }

        public static Authentication Instance { get; } = new Authentication();
    }
}
