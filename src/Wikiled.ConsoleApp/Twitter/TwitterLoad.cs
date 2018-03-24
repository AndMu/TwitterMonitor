using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Caching.Memory;
using NLog;
using Tweetinvi;
using Tweetinvi.Core.Helpers;
using Tweetinvi.Logic.DTO;
using Wikiled.Common.Logging;
using Wikiled.Console.Arguments;
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
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly IJsonObjectConverter jsonConvert;

        private readonly ChunkProcessor processor = new ChunkProcessor();

        private RedisPersistency persistency;

        private int total;

        public TwitterLoad()
        {
            jsonConvert = TweetinviContainer.Resolve<IJsonObjectConverter>();
        }

        public override string Name => "load";

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
                persistency = new RedisPersistency(link, new Microsoft.Extensions.Caching.Memory.MemoryCache(new MemoryCacheOptions()));
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
                await persistency.Save(tweet).ConfigureAwait(false);
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
            PerformanceMonitor monitor = new PerformanceMonitor(files.Length);
            using (Observable.Interval(TimeSpan.FromSeconds(30)).Subscribe(item => log.Info(monitor)))
            {
                var inputBlock = new BufferBlock<ProcessingChunk<string>>(new DataflowBlockOptions { BoundedCapacity = 1000000 });
                var deserializeBlock = new TransformBlock<ProcessingChunk<string>, ProcessingChunk<TweetDTO>>(
                    json => new ProcessingChunk<TweetDTO>(json.FileName, json.ChunkId, json.TotalChunks, jsonConvert.DeserializeObject<TweetDTO>(json.Data)),
                    new ExecutionDataflowBlockOptions
                        {
                            BoundedCapacity = 2,
                            MaxDegreeOfParallelism = Environment.ProcessorCount
                        });
                var outputBlock = new ActionBlock<ProcessingChunk<TweetDTO>>(
                    Deserialized,
                    new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = Environment.ProcessorCount });

                inputBlock.LinkTo(deserializeBlock, new DataflowLinkOptions { PropagateCompletion = true });
                deserializeBlock.LinkTo(outputBlock, new DataflowLinkOptions { PropagateCompletion = true });

                foreach (var file in files)
                {
                    try
                    {
                        var data = FilePersistency.Load(file);
                        for (int i = 0; i < data.Length; i++)
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
                await Task.WhenAll(inputBlock.Completion, outputBlock.Completion).ConfigureAwait(false);
            }
        }
    }
}
