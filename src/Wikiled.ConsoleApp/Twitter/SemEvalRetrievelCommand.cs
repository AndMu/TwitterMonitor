using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using NLog;
using Wikiled.Core.Utility.Arguments;

namespace Wikiled.ConsoleApp.Twitter
{
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
            throw new NotImplementedException();
        }
    }
}
