using System;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using ProtoBuf;
using Wikiled.Core.Utility.Arguments;
using Wikiled.Core.Utility.IO;
using Wikiled.Twitter.Persistency.Data;

namespace Wikiled.Twitter.Persistency
{
    public class FilePersistency : IPersistency
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        private readonly IStreamSource streamSource;

        private readonly object syncRoot = new object();

        public FilePersistency(IStreamSource streamSource)
        {
            Guard.NotNull(() => streamSource, streamSource);
            this.streamSource = streamSource;
        }

        public void Save(string json)
        {
            Guard.NotNullOrEmpty(() => json, json);
            try
            {
                var data = new RawTweetData();
                data.Data = Encoding.UTF8.GetBytes(json).Zip();
                lock (syncRoot)
                {
                    var stream = streamSource.GetStream();
                    Serializer.SerializeWithLengthPrefix(stream, data, PrefixStyle.Base128, 1);
                    stream.Flush();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public static string[] Load(string fileName)
        {
            log.Debug("Load [{0}]", fileName);
            using (var stream = File.OpenRead(fileName))
            {
                return Serializer.DeserializeItems<RawTweetData>(stream, PrefixStyle.Base128, 1)
                    .Select(item => Encoding.UTF8.GetString(item.Data.UnZip()))
                    .ToArray();
            }
        }
    }
}