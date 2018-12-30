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
using Wikiled.ConsoleApp.Commands.Config;
using Wikiled.Twitter.Discovery;
using Wikiled.Twitter.Security;

namespace Wikiled.ConsoleApp.Commands
{
    public class DiscoveryCommand : Command
    {
        private readonly ILogger<DiscoveryCommand> log;

        private readonly Func<DiscoveryRequest, IMessageDiscovery> discoveryFactory;

        private readonly IAuthentication auth;

        private readonly DiscoveryConfig config;

        public DiscoveryCommand(ILogger<DiscoveryCommand> log,
                                IAuthentication authentication,
                                Func<DiscoveryRequest, IMessageDiscovery> discoveryFactory,
                                DiscoveryConfig config)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this.discoveryFactory = discoveryFactory ?? throw new ArgumentNullException(nameof(discoveryFactory));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            auth = authentication ?? throw new ArgumentNullException(nameof(authentication));
        }

        protected override Task Execute(CancellationToken token)
        {
            log.LogInformation("Starting twitter monitoring...");
            var keywords = string.IsNullOrEmpty(config.Topics) ? new string[] { } : config.Topics.Split(',');
            if (config.Topics.Length == 0)
            {
                throw new NotSupportedException("Invalid selection");
            }

            var discovery = discoveryFactory(new DiscoveryRequest(keywords));
            var cred = auth.Authenticate();
            using (var streamWriter = new StreamWriter(config.Out, true, new UTF8Encoding(false)))
            using (var csvDataTarget = new CsvWriter(streamWriter))
            {
                Auth.ExecuteOperationWithCredentials(
                    cred,
                    () =>
                    {
                        foreach (var item in discovery.Process())
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
