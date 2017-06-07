namespace Wikiled.ConsoleApp.Twitter
{
    public interface IProcessingChunk
    {
        string FileName { get; }

        int ChunkId { get; }

        int TotalChunks { get; }
    }
}