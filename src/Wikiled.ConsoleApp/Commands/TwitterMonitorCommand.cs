using Microsoft.Extensions.Logging;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wikiled.Console.Arguments;
using Wikiled.ConsoleApp.Commands.Config;
using Wikiled.Twitter.Persistency;
using Wikiled.Twitter.Streams;

namespace Wikiled.ConsoleApp.Commands
{
    /// <summary>
    ///     monitor -Out=c:\twitter -Keywords=#Trump,#NeverTrump,#DonaldTrump,#Trump2016,@realDonaldTrump -People=realDonaldTrump
    /// </summary>
    public class TwitterMonitorCommand : Command
    {
        private readonly ILogger<TwitterMonitorCommand> log;

        private readonly IMonitoringStream monitoring;

        private readonly IPersistency persistency;

        private readonly TwitterMonitorConfig config;

        public TwitterMonitorCommand(ILogger<TwitterMonitorCommand> log, TwitterMonitorConfig config, IMonitoringStream monitoring,  IPersistency persistency)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this.monitoring = monitoring ?? throw new ArgumentNullException(nameof(monitoring));
            this.config = config;
            this.persistency = persistency;
        }

        protected override async Task Execute(CancellationToken token)
        {
            log.LogInformation("Starting twitter monitoring...");

            string[] keywords = string.IsNullOrEmpty(config.Keywords) ? new string[] { } : config.Keywords.Split(',');
            string[] follow = string.IsNullOrEmpty(config.People) ? new string[] { } : config.People.Split(',');
            if (follow.Length == 0 &&
                keywords.Length == 0)
            {
                throw new NotSupportedException("Invalid selection");
            }

            IDisposable subscribtion = monitoring.MessagesReceiving
                .ObserveOn(TaskPoolScheduler.Default)
                .Subscribe(item => persistency.Save(item));
            await monitoring.Start(keywords, follow).ConfigureAwait(false);
            subscribtion.Dispose();
            System.Console.WriteLine("To stop press enter...");
            System.Console.ReadLine();
        }
    }
}
