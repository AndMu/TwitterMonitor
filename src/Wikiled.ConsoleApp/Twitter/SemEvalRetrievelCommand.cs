using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using MoreLinq;
using NLog;
using Tweetinvi;
using Wikiled.Core.Utility.Arguments;
using Wikiled.Core.Utility.Logging;
using Wikiled.Twitter.Security;

namespace Wikiled.ConsoleApp.Twitter
{
    /// <summary>
    /// semeval -Out=c:\Data\SemEval\ -Out=c:\Data\SemEval\result.csv
    /// </summary>
    public class SemEvalRetrievelCommand : Command
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        [Required]
        public string Source { get; set; }

        [Required]
        public string Out { get; set; }

        public override void Execute()
        {
            log.Info("Retrieving SemEval Twitter messages...");
            var allFiles = Directory.GetFiles(Source, "*.tsv", SearchOption.AllDirectories);

            log.Info("Found {0} files...", allFiles.Length);
            Dictionary<long, long> unique = new Dictionary<long, long>();
            foreach (var file in allFiles)
            {
                using (var reader = new StreamReader(file, true))
                {
                    using (var csv = new CsvReader(reader))
                    {
                        csv.Configuration.Delimiter = "\t";
                        csv.ReadHeader();
                        while (csv.Read())
                        {
                            var id = csv.GetField<long>(0);
                            unique[id] = id;
                        }
                    }
                }
            }

            log.Info("Loaded all files and found {0} unique ids ...", unique.Count);
            var auth = new PersistedAuthentication(new PinAuthentication());
            var cred = auth.Authenticate();
            Auth.ExecuteOperationWithCredentials(cred, () => { ProcessMessages(unique); });
            Search.SearchTweets("tweetinvi");
        }

        private void ProcessMessages(Dictionary<long, long> unique)
        {
            log.Info("Processing files...");
            PerformanceMonitor monitor = new PerformanceMonitor(unique.Count, 100);
            using (var streamWrite = new StreamWriter(Out, false, Encoding.UTF8))
            using (var csvTarget = new CsvWriter(streamWrite))
            {
                csvTarget.WriteField("Id");
                csvTarget.WriteField("UserId");
                csvTarget.WriteField("Text");
                csvTarget.NextRecord();

                foreach (var ids in unique.Values.Batch(50))
                {
                    var messages = Tweet.GetTweets(ids.ToArray());
                    foreach (var message in messages)
                    {
                        monitor.Increment();
                        csvTarget.WriteField(message.Id);
                        csvTarget.WriteField(message.CreatedBy.Id);
                        csvTarget.WriteField(message.Text);
                        csvTarget.NextRecord();
                    }
                }
            }
        }
    }
}
