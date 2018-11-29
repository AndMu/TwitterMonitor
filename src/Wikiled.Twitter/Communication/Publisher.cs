using System;
using Microsoft.Extensions.Logging;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using Wikiled.Twitter.Security;

namespace Wikiled.Twitter.Communication
{
    public class Publisher : IPublisher
    {
        private readonly IAuthentication auth;

        private readonly ILogger<Publisher> log;

        public Publisher(ILogger<Publisher> log, IAuthentication auth)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this.auth = auth ?? throw new ArgumentNullException(nameof(auth));
        }

        public void SendPrivate(long userId, string message)
        {
            log.LogInformation("Send message to {0}", userId);
            var authentication = auth.Authenticate();
            Auth.ExecuteOperationWithCredentials(
                authentication,
                () => { Message.PublishMessage("Messages Rocks around here!", userId); });
        }

        public void PublishMessage(IFeedMessage publishMessage)
        {
            log.LogInformation("Publishing message");
            var authentication = auth.Authenticate();
            Auth.ExecuteOperationWithCredentials(
                authentication,
                () =>
                {
                    publishMessage.Prepare();
                    foreach (var message in publishMessage.GenerateMessages())
                    {
                        Publish(message);
                    }
                });
            log.LogInformation("Done!");
        }

        private void Publish(IPublishTweetParameters tweet)
        {
            ITweet message = Tweet.PublishTweet(tweet);
            if (message != null)
            {
                return;
            }

            Tweetinvi.Core.Exceptions.ITwitterException exception = ExceptionHandler.GetLastException();
            if (exception == null)
            {
                return;
            }

            foreach (Tweetinvi.Core.Exceptions.ITwitterExceptionInfo exceptionTwitterExceptionInfo in exception.TwitterExceptionInfos)
            {
                log.LogError(exceptionTwitterExceptionInfo.Message);
            }
        }
    }
}
