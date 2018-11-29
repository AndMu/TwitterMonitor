using System.Collections.Generic;
using Tweetinvi.Parameters;

namespace Wikiled.Twitter.Communication
{
    public interface IFeedMessage
    {
        void Prepare();

        IEnumerable<IPublishTweetParameters> GenerateMessages();
    }
}