using System;
using System.Text;
using Tweetinvi.Models.DTO;
using Wikiled.Text.Analysis.Twitter;

namespace Wikiled.Twitter.Persistency
{
    public class FlatFileSerializer : IPersistency
    {
        private readonly IStreamSource streamSource;

        private readonly object syncRoot = new object();

        private readonly MessageCleanup cleanup = new MessageCleanup();

        public FlatFileSerializer(IStreamSource streamSource)
        {
            this.streamSource = streamSource ?? throw new ArgumentNullException(nameof(streamSource));
        }

        public void Save(ITweetDTO dto)
        {
            var textItem = cleanup.Cleanup(dto.Text);
            string text = $"{dto.CreatedAt}\t{dto.Id}\t{dto.CreatedBy.Id}\t{dto.Retweeted}\t{textItem}\r\n";
            lock (syncRoot)
            {
                var stream = streamSource.GetStream();
                var data = Encoding.UTF8.GetBytes(text);
                stream.Write(data, 0, data.Length);
                stream.Flush();
            }
        }
    }
}
