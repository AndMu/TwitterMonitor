using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using NLog;
using Tweetinvi;
using Wikiled.Console.Arguments;
using Wikiled.Twitter.Discovery;
using Wikiled.Twitter.Security;

namespace Wikiled.ConsoleApp.Twitter
{
    public class DiscoveryCommand : Command
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        [Required]
        public string Topics { get; set; }

        [Required]
        public string Out { get; set; }

        protected override Task Execute(CancellationToken token)
        {
            log.Info("Starting twitter monitoring...");
            string[] keywords = string.IsNullOrEmpty(Topics) ? new string[] { } : Topics.Split(',');
            if (Topics.Length == 0)
            {
                throw new NotSupportedException("Invalid selection");
            }

            RateLimit.RateLimitTrackerMode = RateLimitTrackerMode.TrackAndAwait;
            MessageDiscovery discovery = new MessageDiscovery(keywords);

            var auth = new PersistedAuthentication(new PinConsoleAuthentication(Credentials.Instance.IphoneTwitterCredentials));
            var cred = auth.Authenticate();
            using (var streamWriter = new StreamWriter(Out, true, new UTF8Encoding(false)))
            using (var csvDataTarget = new CsvWriter(streamWriter))
            {
                Auth.ExecuteOperationWithCredentials(
                    cred,
                    () =>
                    {
                        foreach(var item in discovery.Process())
                        {
                            csvDataTarget.WriteField(item.Message.Id);
                            csvDataTarget.WriteField(item.Message.Text);
                            csvDataTarget.NextRecord();
                            streamWriter.Flush();
                        }
                    });
            }

            return Task.CompletedTask;
        }
    }
}
