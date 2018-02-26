using System.IO;
using Wikiled.Common.Arguments;

namespace Wikiled.Twitter.Persistency
{
    public class BasicStreamSource : IStreamSource
    {
        private readonly Stream stream;

        public BasicStreamSource(Stream stream)
        {
            Guard.NotNull(() => stream, stream);
            this.stream = stream;
        }

        public void Dispose()
        {
        }

        public Stream GetStream()
        {
            return stream;
        }
    }
}
