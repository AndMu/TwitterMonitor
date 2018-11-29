using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wikiled.Console.Arguments;
using Wikiled.ConsoleApp.Commands.Config;
using Wikiled.Twitter.Communication;

namespace Wikiled.ConsoleApp.Commands
{
    /// <summary>
    /// publish -Image=c:\1\Laranjeiras.jpg -Text="Sample Image"
    /// </summary>
    public class TestPublishCommand : Command
    {
        private readonly ILogger<TestPublishCommand> log;

        private readonly IPublisher publisher;

        private readonly TestPublishConfig config;

        public TestPublishCommand(ILogger<TestPublishCommand> log, TestPublishConfig config, IPublisher publisher)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this.publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }
        
        protected override Task Execute(CancellationToken token)
        {
            log.LogInformation("Publishing...");
            IFeedMessage message;
            if (string.IsNullOrEmpty(config.Image))
            {
                message = new MultiItemMessage("Test Message", config.Text.Split(' '));
            }
            else
            {
                message = new MediaMessage(config.Text, File.ReadAllBytes(config.Image));
            }

            publisher.PublishMessage(message);
            return Task.CompletedTask;
        }
    }
}
