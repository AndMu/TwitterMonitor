using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Wikiled.Common.Extensions;

namespace Wikiled.Twitter.Persistency
{
    public class TimingStreamSource : IStreamSource
    {
        private readonly TimeSpan fileCreation;

        private readonly ILogger<TimingStreamSource> log;

        private readonly string path;

        private readonly Stopwatch stopwatch = new Stopwatch();

        private int isDisposed;

        private FileStream stream;

        public TimingStreamSource(ILogger<TimingStreamSource> log, string path, TimeSpan fileCreation)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(path));
            }

            log.LogDebug(path);
            this.path = path;
            this.fileCreation = fileCreation;
            this.log = log;
            path.EnsureDirectoryExistence();
        }

        private bool IsDisposed { get => Interlocked.CompareExchange(ref isDisposed, 0, 0) == 1; set => Interlocked.Exchange(ref isDisposed, value ? 1 : 0); }

        public void Dispose()
        {
            log.LogDebug("Dispose");
            IsDisposed = true;
            stream?.Dispose();
        }

        public Stream GetStream()
        {
            if (IsDisposed)
            {
                return null;
            }

            if (stream == null ||
                stopwatch.Elapsed >= fileCreation)
            {
                stopwatch.Restart();
                if (stream != null)
                {
                    stream.Flush();
                    stream.Dispose();
                }

                var fileName = Path.Combine(path, $"data_{DateTime.UtcNow:yyyyMMdd_HHmm}.dat");
                log.LogInformation("Creating: {0}...", fileName);
                stream = new FileStream(fileName, FileMode.Create);
            }

            return stream;
        }
    }
}
