using Microsoft.Extensions.Logging;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wikiled.Console.Arguments;
using Wikiled.Twitter.Persistency;
using Wikiled.Twitter.Streams;

namespace Wikiled.ConsoleApp.Commands
{
    /// <summary>
    ///     monitor -Out=c:\twitter -Keywords=#Trump,#NeverTrump,#DonaldTrump,#Trump2016,@realDonaldTrump -People=realDonaldTrump
    /// </summary>
    public class TwitterMonitorCommand : Command
    {
        private ILogger<TwitterMonitorCommand> log;

        private readonly Func<string, TimeSpan, IStreamSource> sourceFactory;

        private readonly IMonitoringStream monitoring;

        private IPersistencyFactory factory;

        public TwitterMonitorCommand(ILogger<TwitterMonitorCommand> log)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public override string Name => "monitor";

        public string Out { get; set; }

        public string People { get; set; }

        public bool Compress { get; set; }

        public string Keywords { get; set; }

        protected override async Task Execute(CancellationToken token)
        {
            log.LogInformation("Starting twitter monitoring...");
            string path = string.IsNullOrEmpty(Out) ? "out" : Out;
            string[] keywords = string.IsNullOrEmpty(Keywords) ? new string[] { } : Keywords.Split(',');
            string[] follow = string.IsNullOrEmpty(People) ? new string[] { } : People.Split(',');
            if (follow.Length == 0 &&
                keywords.Length == 0)
            {
                throw new NotSupportedException("Invalid selection");
            }

            using (IStreamSource streamSource = sourceFactory(path, TimeSpan.FromHours(1)))
            {
                var persistency = factory.Create(Compress);
                IDisposable subscribtion = monitoring.MessagesReceiving
                    .ObserveOn(TaskPoolScheduler.Default)
                    .Subscribe(item => persistency.Save(item));
                await monitoring.Start(keywords, follow).ConfigureAwait(false);
                System.Console.WriteLine("To stop press enter...");
                System.Console.ReadLine();
                monitoring.Stop();
                subscribtion.Dispose();
            }
        }
    }
}
