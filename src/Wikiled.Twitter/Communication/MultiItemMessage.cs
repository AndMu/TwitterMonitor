using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tweetinvi.Parameters;

namespace Wikiled.Twitter.Communication
{
    public class MultiItemMessage : IFeedMessage
    {
        public MultiItemMessage(string header, string[] messages, int maxLength = 140)
        {
            Header = header;
            Messages = messages;
            MaxLength = maxLength;
        }

        public string Header { get; }

        public int MaxLength { get; }

        public string[] Messages { get; }

        public void Prepare()
        {
        }

        public IEnumerable<IPublishTweetParameters> GenerateMessages()
        {
            return GenerateTextMessages()
                .Select(item => new PublishTweetParameters(item, new PublishTweetOptionalParameters()));
        }

        private IEnumerable<string> GenerateTextMessages()
        {
            StringBuilder builder = null;
            foreach (var message in Messages)
            {
                if (builder == null)
                {
                    builder = new StringBuilder(Header);
                }

                var predicted = builder.Length + message.Length + 1;
                if (predicted > MaxLength)
                {
                    yield return builder.ToString();
                    builder = null;
                }
                else
                {
                    builder.AppendFormat(" {0}", message);
                }
            }

            if (builder != null)
            {
                yield return builder.ToString();
            }
        }
    }
}
