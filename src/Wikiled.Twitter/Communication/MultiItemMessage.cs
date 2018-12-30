using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tweetinvi.Parameters;

namespace Wikiled.Twitter.Communication
{
    public class MultiItemMessage : IFeedMessage
    {
        private readonly string breaks;

        public MultiItemMessage(string header, string[] messages, string breaks = "\r\n", int maxLength = 140)
        {
            Header = header;
            Messages = messages;
            this.breaks = breaks;
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
                    builder = new StringBuilder($"{Header}\r\n");
                }

                var predicted = builder.Length + message.Length + 1;
                if (predicted > MaxLength)
                {
                    yield return builder.ToString();
                    builder = null;
                }
                else
                {
                    builder.AppendFormat("{0}{1}", message, breaks);
                }
            }

            if (builder != null)
            {
                yield return builder.ToString();
            }
        }
    }
}
