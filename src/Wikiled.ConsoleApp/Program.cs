using System;
using System.Threading;
using System.Threading.Tasks;
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
            NLog.LogManager.LoadConfiguration("nlog.config");
            AutoStarter starter = new AutoStarter("Twitter Bot", args);
            starter.Factory.AddNLog();
            starter.RegisterCommand<DiscoveryCommand, DiscoveryConfig>("Discovery");
            starter.RegisterCommand<EnrichCommand, EnrichConfig>("Enrich");
            starter.RegisterCommand<DownloadMessagesCommand, DownloadMessagesConfig>("DownloadMessages");
            starter.RegisterCommand<TwitterLoadCommand, TwitterLoadConfig>("load");
            starter.RegisterCommand<TwitterMonitorCommand, TwitterMonitorConfig>("monitor");
            starter.RegisterCommand<TestPublishCommand, TestPublishConfig>("publish");

            CancellationTokenSource source = new CancellationTokenSource();
            var task = starter.StartAsync(source.Token);
            System.Console.WriteLine("Please press enter to exit...");
            System.Console.ReadLine();
            if (!task.IsCompleted)
            {
                source.Cancel();
                source = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await starter.StopAsync(source.Token).ConfigureAwait(false);
            }
        }
    }
}
