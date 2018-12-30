using Microsoft.Extensions.Logging;
using System;
using System.Security.Authentication;
using Tweetinvi;
using Tweetinvi.Models;
using Wikiled.Common.Utilities.Helpers;

namespace Wikiled.Twitter.Security
{
    public class PinConsoleAuthentication : IAuthentication
    {
        private readonly ILogger<PinConsoleAuthentication> log;

        private readonly ICredentialsSource applicationCredentials;

        public PinConsoleAuthentication(ILogger<PinConsoleAuthentication> log, ICredentialsSource applicationCredentials)
        {
            this.applicationCredentials = applicationCredentials ?? throw new ArgumentNullException(nameof(applicationCredentials));
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public ITwitterCredentials Authenticate()
        {
            // Go to the URL so that Twitter authenticates the user and gives him a PIN code
            var token = applicationCredentials.Resolve();
            var authenticationContext = AuthFlow.InitAuthentication(token);

            // This line is an example, on how to make the user go on the URL
            ExternaApp.OpenUrl(authenticationContext.AuthorizationURL);
            log.LogInformation("Reading console pin");
            Console.WriteLine("Enter your Pin:");
            // Ask the user to enter the pin code given by Twitter
            var pinCode = Console.ReadLine();
            if (string.IsNullOrEmpty(pinCode))
            {
                log.LogError("No pin code entered");
                throw new AuthenticationException();
            }

            // With this pin code it is now possible to get the applicationCredentials back from Twitter
            return AuthFlow.CreateCredentialsFromVerifierCode(pinCode, authenticationContext);
        }
    }
}
