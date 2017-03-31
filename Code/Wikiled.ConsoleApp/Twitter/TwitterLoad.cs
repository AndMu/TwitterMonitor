using System;
using System.IO;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using NLog;
using Tweetinvi;
using Tweetinvi.Core.Helpers;
using Tweetinvi.Logic.DTO;
using Wikiled.Core.Utility.Arguments;
using Wikiled.Core.Utility.Cache;
using Wikiled.Core.Utility.Logging;
using Wikiled.Redis.Config;
using Wikiled.Redis.Logic;
using Wikiled.Twitter.Persistency;

namespace Wikiled.ConsoleApp.Twitter
{
    /// <summary>
    ///     load -Out=E:\Data\Twitter -TypeName=Trump
    /// </summary>
    public class TwitterLoad : Command
    {
        private readonly IJsonObjectConverter jsonConvert;

        private static Logger log = LogManager.GetCurrentClassLogger();

        private RedisPersistency persistency;

        private int total;

        private readonly ChunkProcessor processor = new ChunkProcessor();

        public TwitterLoad()
        {
            jsonConvert = TweetinviContainer.Resolve<IJsonObjectConverter>();
        }

        [Required]
        public string Out { get; set; }

        [Required]
        public string TypeName { get; set; }

        public override void Execute()
        {
            try
            {
                log.Info("Starting twitter loading...");
                RedisLink link = new RedisLink(TypeName, new RedisMultiplexer(new RedisConfiguration("localhost", 6370)));
                link.Open();
                persistency = new RedisPersistency(link, new RuntimeCache(MemoryCache.Default, TimeSpan.FromMinutes(1)));
                var files = Directory.GetFiles(Out, "*.dat", SearchOption.AllDirectories);
                Process(files).Wait();
                link.Dispose();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private async Task Deserialized(ProcessingChunk<TweetDTO> tweetDto)
        {
            try
            {
                if (tweetDto == null)
                {
                    log.Warn("Null tweet");
                    return;
                }

                var processed = Interlocked.Increment(ref total);
                if (processed % 1000000 == 0)
                {
                    log.Info("Processed: {0}", processed);
                }

                var tweet = Tweet.GenerateTweetFromDTO(tweetDto.Data);
                await persistency.Save(tweet);
                processor.Add(tweetDto);
            }
            catch (Exception ex)
            {
                log.Error($"Failed processing chung {tweetDto.ChunkId}/{tweetDto.TotalChunks} in {tweetDto.FileName}");
                log.Error(ex);
            }
        }

        private async Task Process(string[] files)
        {
            PerformanceMonitor monitor = new PerformanceMonitor(files.Length, 10);
            var inputBlock = new BufferBlock<ProcessingChunk<string>>(new DataflowBlockOptions { BoundedCapacity = 1000000 });
            var deserializeBlock = new TransformBlock<ProcessingChunk<string>, ProcessingChunk<TweetDTO>>(
                json => new ProcessingChunk<TweetDTO>(json.FileName, json.ChunkId, json.TotalChunks, jsonConvert.DeserializeObject<TweetDTO>(json.Data)),
                new ExecutionDataflowBlockOptions
                {
                    BoundedCapacity = 2,
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                });
            var outputBlock = new ActionBlock<ProcessingChunk<TweetDTO>>(
                async tweet => await Deserialized(tweet).ConfigureAwait(false),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = Environment.ProcessorCount });

            inputBlock.LinkTo(deserializeBlock, new DataflowLinkOptions { PropagateCompletion = true });
            deserializeBlock.LinkTo(outputBlock, new DataflowLinkOptions { PropagateCompletion = true });

            foreach (var file in files)
            {
                try
                {
                    var data = FilePersistency.Load(file);
                    for(int i = 0; i < data.Length; i++)
                    {
                        await inputBlock.SendAsync(new ProcessingChunk<string>(file, i, data.Length, data[i])).ConfigureAwait(false);
                    }

                    monitor.Increment();
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
            }

            inputBlock.Complete();
            await Task.WhenAll(inputBlock.Completion, outputBlock.Completion);
        }
    }
}
