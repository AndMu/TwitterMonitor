using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using Tweetinvi.Models.DTO;
using Wikiled.Core.Utility.Arguments;
using Wikiled.Core.Utility.IO;
using Wikiled.Twitter.Persistency.Data;

namespace Wikiled.Twitter.Persistency
{
    public class FlatFileSerializer : IPersistency
    {
        private readonly IStreamSource streamSource;

        private readonly object syncRoot = new object();

        public FlatFileSerializer(IStreamSource streamSource)
        {
            Guard.NotNull(() => streamSource, streamSource);
            this.streamSource = streamSource;
        }

        public void Save(ITweetDTO dto)
        {
            lock (syncRoot)
            {
                var stream = streamSource.GetStream();
                string text = $"{dto.Id}\t{dto.Text}\r\n";
                var data = Encoding.UTF8.GetBytes(text);
                stream.Write(data, 0, data.Length);
                stream.Flush();
            }
        }
    }
}
