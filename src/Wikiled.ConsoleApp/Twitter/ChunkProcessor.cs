using System.Collections.Concurrent;
using System.IO;

namespace Wikiled.ConsoleApp.Twitter
{
    public class ChunkProcessor
    {
        private readonly ConcurrentDictionary<string, ConcurrentBag<IProcessingChunk>> table = new ConcurrentDictionary<string, ConcurrentBag<IProcessingChunk>>();

        public void Add(IProcessingChunk chunk)
        {
            if (!table.TryGetValue(chunk.FileName, out var bag))
            {
                bag = new ConcurrentBag<IProcessingChunk>();
                if (!table.TryAdd(chunk.FileName, bag))
                {
                    bag = table[chunk.FileName];
                }
            }

            bag.Add(chunk);
            if (bag.Count == chunk.TotalChunks)
            {
                File.Delete(chunk.FileName);
                table.TryRemove(chunk.FileName, out bag);
            }
        }
    }
}
