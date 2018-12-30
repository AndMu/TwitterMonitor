using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Tweetinvi;
using Wikiled.Common.Logging;
using Wikiled.Console.Arguments;
using Wikiled.ConsoleApp.Commands.Config;
using Wikiled.Text.Analysis.Twitter;
using Wikiled.Twitter.Discovery;
using Wikiled.Twitter.Security;

namespace Wikiled.ConsoleApp.Commands
{
    /// <summary>
    /// DownloadMessages -Ids=e:\Source\sanders-twitter-0.2\corpus_out.csv -out=.\Messages.csv -Clean
    /// </summary>
    public class DownloadMessagesCommand : Command
    {
        private ILogger<DownloadMessagesCommand> log;

        private readonly IMessagesDownloader downloader;

        private readonly IAuthentication auth;

        private DownloadMessagesConfig config;

        public DownloadMessagesCommand(ILogger<DownloadMessagesCommand> log, IAuthentication auth, IMessagesDownloader downloader, DownloadMessagesConfig config)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this.downloader = downloader ?? throw new ArgumentNullException(nameof(downloader));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.auth = auth ?? throw new ArgumentNullException(nameof(auth));
        }

        protected override Task Execute(CancellationToken token)
        {
            log.LogInformation("Downloading message...");
            var downloadMessages = File.ReadLines(config.Ids).Select(long.Parse).ToArray();
            log.LogInformation("Total messages to download: {0}", downloadMessages.Length);
            var cred = auth.Authenticate();
            var extractor = new MessageCleanup();
            var monitor = new PerformanceMonitor(downloadMessages.Length);
            using (var streamWriter = new StreamWriter(config.Out, false, new UTF8Encoding(false)))
            using (var csvDataTarget = new CsvWriter(streamWriter))
            {
                csvDataTarget.WriteField("Id");
                csvDataTarget.WriteField("Date");
                csvDataTarget.WriteField("Author");
                csvDataTarget.WriteField("Message");
                csvDataTarget.NextRecord();

                Auth.ExecuteOperationWithCredentials(
                    cred,
                    () =>
                        {
                            using (Observable.Interval(TimeSpan.FromSeconds(30)).Subscribe(item => log.LogInformation(monitor.ToString())))
                            {
                                downloader.Download(downloadMessages)
                                          .ToObservable()
                                          .Select(
                                              item =>
                                                  {
                                                      try
                                                      {
                                                          csvDataTarget.WriteField(item.Id);
                                                          csvDataTarget.WriteField(item.CreatedAt);
                                                          csvDataTarget.WriteField(item.CreatedBy.Id);
                                                          var text = item.Text;
                                                          if (config.Clean)
                                                          {
                                                              text = extractor.Cleanup(text);
                                                          }

                                                          csvDataTarget.WriteField(text);
                                                          csvDataTarget.NextRecord();
                                                          monitor.Increment();
                                                      }
                                                      catch (Exception e)
                                                      {
                                                          log.LogError(e, "Error");
                                                      }

                                                      return item;
                                                  })
                                          .LastOrDefaultAsync()
                                          .Wait();
                            }
                        });
            }

            return Task.CompletedTask;
        }
    }
}
