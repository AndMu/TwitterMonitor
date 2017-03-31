using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NLog;
using Wikiled.Core.Utility.Arguments;
using Wikiled.Core.Utility.Extensions;

namespace Wikiled.Twitter.Persistency
{
    public class TimingStreamSource : IStreamSource
    {
        private readonly string path;

        private FileStream stream;

        private readonly Stopwatch stopwatch = new Stopwatch();

        private readonly TimeSpan fileCreation;

        private int isDisposed;

        private static Logger log = LogManager.GetCurrentClassLogger();

        public TimingStreamSource(string path, TimeSpan fileCreation)
        {
            log.Debug(path);
            this.path = path;
            this.fileCreation = fileCreation;
            Guard.NotNullOrEmpty(() => path, path);
            path.EnsureDirectoryExistence();
        }

          private bool IsDisposed
        {
            get
            {
                return Interlocked.CompareExchange(ref isDisposed, 0, 0) == 1;
            }
            set
            {
                Interlocked.Exchange(ref isDisposed, value ? 1 : 0);
            }
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

                var fileName = Path.Combine(path, $"data_{DateTime.UtcNow.ToString("yyyyMMdd_HHmm")}.dat");
                log.Info("Creating: {0}...", fileName);
                stream = new FileStream(fileName, FileMode.Create);
            }

            return stream;
        }

        public void Dispose()
        {
            log.Debug("Dispose");
            IsDisposed = true;
            stream?.Dispose();
        }
    }
}
