using Microsoft.Extensions.Logging;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace Wikiled.Twitter.Communication
{
    public class Publisher
    {
        private readonly ITwitterCredentials cred;

        private readonly ILogger<Publisher> log;

        public Publisher(ILogger<Publisher> log, ITwitterCredentials cred)
        {
            this.log = log;
            this.cred = cred;
        }

        public void PublishMessage(string text)
        {
            log.LogInformation("Publishing message");
            Auth.ExecuteOperationWithCredentials(
                cred,
                () =>
                {
                    ITweet message = Tweet.PublishTweet(text, new PublishTweetOptionalParameters());
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
                });
        }
    }
}
