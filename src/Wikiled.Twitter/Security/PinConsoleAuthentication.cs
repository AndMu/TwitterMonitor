using System;
using System.Diagnostics;
using System.Security.Authentication;
using NLog;
using Tweetinvi;
using Tweetinvi.Models;

namespace Wikiled.Twitter.Security
{
    public class PinConsoleAuthentication : IAuthentication
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public ITwitterCredentials Authenticate(ITwitterCredentials applicationCredentials)
        {
            // Go to the URL so that Twitter authenticates the user and gives him a PIN code
            var authenticationContext = AuthFlow.InitAuthentication(applicationCredentials);

            // This line is an example, on how to make the user go on the URL
            Process.Start(authenticationContext.AuthorizationURL);
            log.Info("Enter your Pin:");

            // Ask the user to enter the pin code given by Twitter
            var pinCode = Console.ReadLine();
            if (string.IsNullOrEmpty(pinCode))
            {
                log.Error("No pin code entered");
                throw new AuthenticationException();
            }

            // With this pin code it is now possible to get the applicationCredentials back from Twitter
            return  AuthFlow.CreateCredentialsFromVerifierCode(pinCode, authenticationContext);
        }
    }
}
