using System;
using System.Threading.Tasks;
using Tweetinvi.Models;

namespace Wikiled.Twitter.Streams
{
    public interface IMonitoringStream : IDisposable
    {
        bool IsActive { get; }

        LanguageFilter[] LanguageFilters { get; set; }

        long TotalReceived { get; }

        Task Start(string[] keywords, string[] follows);

        void Stop();

        void AddTrack(string keyword);

        void RemoveTrack(string keyword);
    }
}