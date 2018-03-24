using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using CsvHelper;
using NLog;
using Tweetinvi;
using Wikiled.Common.Logging;
using Wikiled.Console.Arguments;
using Wikiled.Text.Analysis.Twitter;
using Wikiled.Twitter.Discovery;
using Wikiled.Twitter.Security;

namespace Wikiled.ConsoleApp.Twitter
{
    /// <summary>
    /// DownloadMessages -Ids=e:\Source\sanders-twitter-0.2\corpus_out.csv -out=.\Messages.csv -Clean
    /// </summary>
    public class DownloadMessagesCommand : Command
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        [Required]
        public string Ids { get; set; }

        [Required]
        public string Out { get; set; }

        public bool Clean { get; set; }

        public override void Execute()
        {
            log.Info("Downloading message...");
            var downloadMessages = File.ReadLines(Ids).Select(long.Parse).ToArray();
            log.Info("Total messages to download: {0}", downloadMessages.Length);
            MessagesDownloader downloader = new MessagesDownloader();
            RateLimit.RateLimitTrackerMode = RateLimitTrackerMode.TrackAndAwait;
            MessageCleanup extractor = new MessageCleanup();
            var auth = new PersistedAuthentication(new PinConsoleAuthentication());
            var cred = auth.Authenticate(Credentials.Instance.IphoneTwitterCredentials);
            var monitor = new PerformanceMonitor(downloadMessages.Length);
            using (var streamWriter = new StreamWriter(Out, false, new UTF8Encoding(false)))
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
                            using (Observable.Interval(TimeSpan.FromSeconds(30)).Subscribe(item => log.Info(monitor)))
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
                                                          if (Clean)
                                                          {
                                                              text = extractor.Cleanup(text);
                                                          }

                                                          csvDataTarget.WriteField(text);
                                                          csvDataTarget.NextRecord();
                                                          monitor.Increment();
                                                      }
                                                      catch (Exception e)
                                                      {
                                                          log.Error(e);
                                                      }

                                                      return item;
                                                  })
                                          .LastOrDefaultAsync()
                                          .Wait();
                            }
                        });
            }
        }
    }
}
