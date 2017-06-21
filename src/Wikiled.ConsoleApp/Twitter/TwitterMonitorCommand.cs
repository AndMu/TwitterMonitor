using System;
using NLog;
using Wikiled.Core.Utility.Arguments;
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

        public string Out { get; set; }

        public string People { get; set; }

        public string Keywords { get; set; }

        public override void Execute()
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
            using (monitoringStream = new MonitoringStream(new FilePersistency(streamSource), new PersistedAuthentication(new PinAuthentication())))
            {
                monitoringStream.Start(keywords, follow);
                Console.WriteLine("To stop press enter...");
                Console.ReadLine();
                monitoringStream.Stop();
            }
        }
    }
}
