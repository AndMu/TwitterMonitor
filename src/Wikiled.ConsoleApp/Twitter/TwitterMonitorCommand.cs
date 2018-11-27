using Microsoft.Extensions.Logging;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wikiled.Console.Arguments;
using Wikiled.Twitter.Persistency;
using Wikiled.Twitter.Security;
using Wikiled.Twitter.Streams;

namespace Wikiled.ConsoleApp.Twitter
{
    /// <summary>
    ///     monitor -Out=c:\twitter -Keywords=#Trump,#NeverTrump,#DonaldTrump,#Trump2016,@realDonaldTrump -People=realDonaldTrump
    /// </summary>
    public class TwitterMonitorCommand : Command
    {
        private ILogger<TwitterMonitorCommand> log = Program.LoggingFactory.CreateLogger<TwitterMonitorCommand>();

        private MonitoringStream monitoringStream;

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

            using (TimingStreamSource streamSource =
                new TimingStreamSource(Program.LoggingFactory.CreateLogger<TimingStreamSource>(),
                                       path,
                                       TimeSpan.FromHours(1)))
            using (monitoringStream = new MonitoringStream(Program.LoggingFactory.CreateLogger<MonitoringStream>(),
                                                           new PersistedAuthentication(Program.LoggingFactory.CreateLogger<PersistedAuthentication>(),
                                                                                       new PinConsoleAuthentication(Credentials.Instance.IphoneTwitterCredentials))))
            {
                var persistency = Compress ? new FilePersistency(streamSource) : new FlatFileSerializer(streamSource);
                IDisposable subscribtion = monitoringStream.MessagesReceiving
                    .ObserveOn(TaskPoolScheduler.Default)
                    .Subscribe(item => persistency.Save(item));
                await monitoringStream.Start(keywords, follow).ConfigureAwait(false);
                System.Console.WriteLine("To stop press enter...");
                System.Console.ReadLine();
                monitoringStream.Stop();
                subscribtion.Dispose();
            }
        }
    }
}
