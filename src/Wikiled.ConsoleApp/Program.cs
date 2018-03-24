using Wikiled.Console.Arguments;

namespace Wikiled.ConsoleApp
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            AutoStarter starter = new AutoStarter("Twitter Bot");
            starter.Start(args);
        }
    }
}
