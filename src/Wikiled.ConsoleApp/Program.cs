using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Wikiled.Console.Arguments;
using Wikiled.ConsoleApp.Commands;
using Wikiled.ConsoleApp.Twitter;

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
            starter.Register<DiscoveryCommand>("Discovery");
            starter.Register<EnrichCommand>("Enrich");
            starter.Register<DownloadMessagesCommand>("DownloadMessages");
            starter.Register<TwitterLoad>("Load");
            starter.Register<TwitterMonitorCommand>("monitor");
            await starter.Start(args, CancellationToken.None).ConfigureAwait(false);
        }
    }
}
