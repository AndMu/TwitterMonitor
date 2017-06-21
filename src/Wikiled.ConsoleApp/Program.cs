using System;
using System.Linq;
using System.Reflection;
using NLog;
using Wikiled.ConsoleApp.Twitter;
using Wikiled.Core.Utility.Arguments;

namespace Wikiled.ConsoleApp
{
    public static class Program
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            log.Info("Starting {0} version utility...", Assembly.GetExecutingAssembly().GetName().Version);
            if (args.Length == 0)
            {
                log.Warn("Please specify arguments");
                return;
            }

            try
            {
                Command command;
                if (string.Compare(args[0], "monitor", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    command = new TwitterMonitorCommand();
                }
                else if (string.Compare(args[0], "semeval", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    command = new SemEvalRetrievelCommand();
                }
                else if (string.Compare(args[0], "load", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    command = new TwitterLoad();
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Root argument -" + args[0]);
                }

                command.ParseArguments(args.Skip(1)); // or CommandLineParser.ParseArguments(c, args.Skip(1))
                command.Execute();
            }
            catch (Exception ex)
            {
                log.Error(ex);
                Console.ReadLine();
            }
        }
    }
}
