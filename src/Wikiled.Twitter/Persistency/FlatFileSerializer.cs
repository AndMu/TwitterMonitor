using System.Text;
using Tweetinvi.Models.DTO;
using Wikiled.Common.Arguments;
using Wikiled.Text.Analysis.Twitter;

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
            string text = $"{dto.CreatedAt}\t{dto.Id}\t{dto.CreatedBy.Id}\t{dto.Retweeted}\t{dto.Text}\r\n";
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
