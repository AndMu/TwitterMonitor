using System;
using System.Threading.Tasks;
using NLog;
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
        private static Logger log = LogManager.GetCurrentClassLogger();

        private MonitoringStream monitoringStream;

        public override string Name => "monitor";

        public string Out { get; set; }

        public string People { get; set; }

        public bool Compress { get; set; }

        public string Keywords { get; set; }

        public override Task Execute()
        {
            log.Info("Starting twitter monitoring...");
            string path = string.IsNullOrEmpty(Out) ? "out" : Out;
            string[] keywords = string.IsNullOrEmpty(Keywords) ? new string[] { } : Keywords.Split(',');
            string[] follow = string.IsNullOrEmpty(People) ? new string[] { } : People.Split(',');
            if (follow.Length == 0 &&
                keywords.Length == 0)
            {
                throw new NotSupportedException("Invalid selection");
            }

            using (var streamSource = new TimingStreamSource(path, TimeSpan.FromHours(1)))
            using (monitoringStream = new MonitoringStream(
                       Compress ? new FilePersistency(streamSource) : (IPersistency)new FlatFileSerializer(streamSource), 
                       new PersistedAuthentication(new PinConsoleAuthentication())))
            {
                monitoringStream.Start(keywords, follow);
                System.Console.WriteLine("To stop press enter...");
                System.Console.ReadLine();
                monitoringStream.Stop();
            }

            return Task.CompletedTask;
         }
    }
}
