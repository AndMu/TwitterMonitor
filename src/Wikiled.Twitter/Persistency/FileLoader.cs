using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using ProtoBuf;
using Wikiled.Common.Helpers;
using Wikiled.Twitter.Persistency.Data;

namespace Wikiled.Twitter.Persistency
{
    public class FileLoader : IFileLoader
    {
        private readonly ILogger<FileLoader> log;

        public FileLoader(ILogger<FileLoader> log)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public string[] Load(string fileName)
        {
            log.LogDebug("Load [{0}]", fileName);
            using (var stream = File.OpenRead(fileName))
            {
                return Serializer.DeserializeItems<RawTweetData>(stream, PrefixStyle.Base128, 1).Select(item => Encoding.UTF8.GetString(item.Data.UnZip())).ToArray();
            }
        }
    }
}
