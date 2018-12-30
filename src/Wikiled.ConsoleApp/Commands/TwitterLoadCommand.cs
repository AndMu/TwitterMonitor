using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using Tweetinvi;
using Tweetinvi.Core.Helpers;
using Tweetinvi.Logic.DTO;
using Wikiled.Common.Logging;
using Wikiled.Console.Arguments;
using Wikiled.ConsoleApp.Commands.Config;
using Wikiled.ConsoleApp.Twitter;
using Wikiled.Twitter.Persistency;

namespace Wikiled.ConsoleApp.Commands
{
    /// <summary>
    ///     load -Out=E:\Data\Twitter -TypeName=Trump
    /// </summary>
    public class TwitterLoadCommand : Command
    {
        private readonly ILogger<TwitterLoadCommand> log;

        private readonly IJsonObjectConverter jsonConvert;

        private readonly ChunkProcessor processor = new ChunkProcessor();

        private readonly IRedisPersistency persistency;

        private readonly IFileLoader fileLoader;

        private int total;

        private readonly TwitterLoadConfig config;

        public TwitterLoadCommand(ILogger<TwitterLoadCommand> log, TwitterLoadConfig config, IRedisPersistency persistency, IFileLoader fileLoader)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.persistency = persistency ?? throw new ArgumentNullException(nameof(persistency));
            this.fileLoader = fileLoader ?? throw new ArgumentNullException(nameof(fileLoader));
            jsonConvert = TweetinviContainer.Resolve<IJsonObjectConverter>();
        }

        protected override async Task Execute(CancellationToken token)
        {
            try
            {
                log.LogInformation("Starting twitter loading...");
                var files = Directory.GetFiles(config.Out, "*.dat", SearchOption.AllDirectories);
                await Process(files).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed");
            }
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

                var processed = Interlocked.Increment(ref total);
                if (processed % 1000000 == 0)
                {
                    log.LogInformation("Processed: {0}", processed);
                }

                var tweet = Tweet.GenerateTweetFromDTO(tweetDto.Data);
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
            var monitor = new PerformanceMonitor(files.Length);
            using (Observable.Interval(TimeSpan.FromSeconds(30)).Subscribe(item => log.LogInformation(monitor.ToString())))
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
                        var data = fileLoader.Load(file);
                        for (var i = 0; i < data.Length; i++)
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
