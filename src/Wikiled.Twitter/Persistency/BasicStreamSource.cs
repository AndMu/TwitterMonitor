using System;
using System.IO;

namespace Wikiled.Twitter.Persistency
{
    public class BasicStreamSource : IStreamSource
    {
        private readonly Stream stream;

        public BasicStreamSource(Stream stream)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
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
