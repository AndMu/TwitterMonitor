namespace Wikiled.ConsoleApp.Twitter
{
    public class ProcessingChunk<T> : IProcessingChunk
    {
        public ProcessingChunk(string fileName, int chunkId, int totalChunks, T data)
        {
            FileName = fileName;
            ChunkId = chunkId;
            TotalChunks = totalChunks;
            Data = data;
        }

        public T Data { get; }

        public string FileName { get; }

        public int ChunkId { get; }

        public int TotalChunks { get; }
    }
}
