using System;
using System.Text;
using Microsoft.Extensions.Logging;
using ProtoBuf;
using Tweetinvi;
using Tweetinvi.Models.DTO;
using Wikiled.Common.Helpers;
using Wikiled.Twitter.Persistency.Data;

namespace Wikiled.Twitter.Persistency
{
    public class FilePersistency : IPersistency
    {
        private readonly ILogger<FilePersistency> log;

        private readonly IStreamSource streamSource;

        private readonly object syncRoot = new object();

        public FilePersistency(ILogger<FilePersistency> log, IStreamSource streamSource)
        {
            this.streamSource = streamSource ?? throw new ArgumentNullException(nameof(streamSource));
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public void Save(ITweetDTO tweet)
        {
            if (tweet == null)
            {
                throw new ArgumentNullException(nameof(tweet));
            }

            try
            {
                var data = new RawTweetData();
                data.Data = Encoding.UTF8.GetBytes(tweet.ToJson()).Zip();
                lock (syncRoot)
                {
                    var stream = streamSource.GetStream();
                    Serializer.SerializeWithLengthPrefix(stream, data, PrefixStyle.Base128, 1);
                    stream.Flush();
                }
            }
            catch(Exception ex)
            {
                log.LogError(ex, "Error");
            }
        }
    }
}
