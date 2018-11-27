using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Wikiled.Console.Arguments;

namespace Wikiled.ConsoleApp
{
    public static class Program
    {
        public static LoggerFactory LoggingFactory { get; private set; }
        public static async Task Main(string[] args)
        {
            LoggingFactory = new LoggerFactory();
            NLog.LogManager.LoadConfiguration("nlog.config");
            LoggingFactory.AddNLog();
            AutoStarter starter = new AutoStarter("Twitter Bot", args);
            await starter.Start(CancellationToken.None).ConfigureAwait(false);
        }
    }
}
