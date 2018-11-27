using CsvHelper;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tweetinvi;
using Wikiled.Console.Arguments;
using Wikiled.Twitter.Discovery;
using Wikiled.Twitter.Security;

namespace Wikiled.ConsoleApp.Twitter
{
    public class DiscoveryCommand : Command
    {
        private ILogger<DiscoveryCommand> log = Program.LoggingFactory.CreateLogger<DiscoveryCommand>();

        [Required]
        public string Topics { get; set; }

        [Required]
        public string Out { get; set; }

        protected override Task Execute(CancellationToken token)
        {
            log.LogInformation("Starting twitter monitoring...");
            string[] keywords = string.IsNullOrEmpty(Topics) ? new string[] { } : Topics.Split(',');
            if (Topics.Length == 0)
            {
                throw new NotSupportedException("Invalid selection");
            }

            RateLimit.RateLimitTrackerMode = RateLimitTrackerMode.TrackAndAwait;
            MessageDiscovery discovery = new MessageDiscovery(Program.LoggingFactory.CreateLogger<MessageDiscovery>(), keywords);

            PersistedAuthentication auth = new PersistedAuthentication(
                Program.LoggingFactory.CreateLogger<PersistedAuthentication>(),
                new PinConsoleAuthentication(
                    Program.LoggingFactory.CreateLogger<PinConsoleAuthentication>(),
                    Credentials.Instance.IphoneTwitterCredentials));
            Tweetinvi.Models.ITwitterCredentials cred = auth.Authenticate();
            using (StreamWriter streamWriter = new StreamWriter(Out, true, new UTF8Encoding(false)))
            using (CsvWriter csvDataTarget = new CsvWriter(streamWriter))
            {
                Auth.ExecuteOperationWithCredentials(
                    cred,
                    () =>
                    {
                        foreach ((Tweetinvi.Models.ITweet Message, string Topic) item in discovery.Process())
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
