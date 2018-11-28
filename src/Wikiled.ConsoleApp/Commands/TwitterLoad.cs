using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Tweetinvi;
using Tweetinvi.Core.Helpers;
using Tweetinvi.Logic.DTO;
using Wikiled.Common.Logging;
using Wikiled.Console.Arguments;
using Wikiled.ConsoleApp.Twitter;
using Wikiled.Redis.Config;
using Wikiled.Redis.Logic;
using Wikiled.Twitter.Persistency;

namespace Wikiled.ConsoleApp.Commands
{
    /// <summary>
    ///     load -Out=E:\Data\Twitter -TypeName=Trump
    /// </summary>
    public class TwitterLoad : Command
    {
        private ILogger<TwitterLoad> log;

        private readonly IJsonObjectConverter jsonConvert;

        private readonly ChunkProcessor processor = new ChunkProcessor();

        private RedisPersistency persistency;

        private int total;

        public TwitterLoad(ILogger<TwitterLoad> log)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            jsonConvert = TweetinviContainer.Resolve<IJsonObjectConverter>();
        }

        public override string Name => "load";

        [Required]
        public string Out { get; set; }

        [Required]
        public string TypeName { get; set; }

        protected override Task Execute(CancellationToken token)
        {
            try
            {
                log.LogInformation("Starting twitter loading...");
                RedisLink link = new RedisLink(TypeName, new RedisMultiplexer(new RedisConfiguration("localhost", 6370)));
                link.Open();
                persistency = new RedisPersistency(Program.LoggingFactory.CreateLogger<RedisPersistency>(), link, new MemoryCache(new MemoryCacheOptions()));
                string[] files = Directory.GetFiles(Out, "*.dat", SearchOption.AllDirectories);
                Process(files).Wait();
                link.Dispose();
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed");
            }

            return Task.CompletedTask;
        }

        private async Task Deserialized(ProcessingChunk<TweetDTO> tweetDto)
        {
            try
            {
                if (tweetDto == null)
                {
                    log.LogWarning("Null tweet");
                    return;
                }

                int processed = Interlocked.Increment(ref total);
                if (processed % 1000000 == 0)
                {
                    log.LogInformation("Processed: {0}", processed);
                }

                Tweetinvi.Models.ITweet tweet = Tweet.GenerateTweetFromDTO(tweetDto.Data);
                await persistency.Save(tweet).ConfigureAwait(false);
                processor.Add(tweetDto);
            }
            catch (Exception ex)
            {
                log.LogError($"Failed processing chung {tweetDto.ChunkId}/{tweetDto.TotalChunks} in {tweetDto.FileName}");
                log.LogError(ex, "Failed");
            }
        }

        private async Task Process(string[] files)
        {
            PerformanceMonitor monitor = new PerformanceMonitor(files.Length);
            using (Observable.Interval(TimeSpan.FromSeconds(30)).Subscribe(item => log.LogInformation(monitor.ToString())))
            {
                BufferBlock<ProcessingChunk<string>> inputBlock = new BufferBlock<ProcessingChunk<string>>(new DataflowBlockOptions { BoundedCapacity = 1000000 });
                TransformBlock<ProcessingChunk<string>, ProcessingChunk<TweetDTO>> deserializeBlock = new TransformBlock<ProcessingChunk<string>, ProcessingChunk<TweetDTO>>(
                    json => new ProcessingChunk<TweetDTO>(json.FileName, json.ChunkId, json.TotalChunks, jsonConvert.DeserializeObject<TweetDTO>(json.Data)),
                    new ExecutionDataflowBlockOptions
                    {
                        BoundedCapacity = 2,
                        MaxDegreeOfParallelism = Environment.ProcessorCount
                    });
                ActionBlock<ProcessingChunk<TweetDTO>> outputBlock = new ActionBlock<ProcessingChunk<TweetDTO>>(
                    Deserialized,
                    new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = Environment.ProcessorCount });

                inputBlock.LinkTo(deserializeBlock, new DataflowLinkOptions { PropagateCompletion = true });
                deserializeBlock.LinkTo(outputBlock, new DataflowLinkOptions { PropagateCompletion = true });

                foreach (string file in files)
                {
                    try
                    {
                        string[] data = new FileLoader(Program.LoggingFactory.CreateLogger<FileLoader>()).Load(file);
                        for (int i = 0; i < data.Length; i++)
                        {
                            await inputBlock.SendAsync(new ProcessingChunk<string>(file, i, data.Length, data[i])).ConfigureAwait(false);
                        }

                        monitor.Increment();
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, "Failed");
                    }
                }

                inputBlock.Complete();
                await Task.WhenAll(inputBlock.Completion, outputBlock.Completion).ConfigureAwait(false);
            }
        }
    }
}
