using System.Threading;
using System.Threading.Tasks;
using Wikiled.Console.Arguments;

namespace Wikiled.ConsoleApp
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            AutoStarter starter = new AutoStarter("Twitter Bot", args);
            await starter.Start(CancellationToken.None).ConfigureAwait(false);
        }
    }
}
