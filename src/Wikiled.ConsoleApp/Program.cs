using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Wikiled.Console.Arguments;
using Wikiled.ConsoleApp.Commands;
using Wikiled.ConsoleApp.Commands.Config;

namespace Wikiled.ConsoleApp
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var loggingFactory = new LoggerFactory();
            NLog.LogManager.LoadConfiguration("nlog.config");
            loggingFactory.AddNLog();
            AutoStarter starter = new AutoStarter(loggingFactory, "Twitter Bot");
            starter.RegisterCommand<DiscoveryCommand, DiscoveryConfig>("Discovery");
            starter.RegisterCommand<EnrichCommand, EnrichConfig>("Enrich");
            starter.RegisterCommand<DownloadMessagesCommand, DownloadMessagesConfig>("DownloadMessages");
            starter.RegisterCommand<TwitterLoadCommand, TwitterLoadConfig>("Load");
            starter.RegisterCommand<TwitterMonitorCommand, TwitterMonitorConfig>("monitor");
            await starter.Start(args, CancellationToken.None).ConfigureAwait(false);
        }
    }
}
