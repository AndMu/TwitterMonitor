using System;
using System.Threading.Tasks;
using Tweetinvi.Models;
using Tweetinvi.Models.DTO;

namespace Wikiled.Twitter.Streams
{
    public interface IMonitoringStream : IDisposable
    {
        IObservable<ITweetDTO> MessagesReceiving { get; }

        bool IsActive { get; }

        LanguageFilter[] LanguageFilters { get; set; }

        long TotalReceived { get; }

        void TrackUser(string follow);

        Task Start(string[] keywords, string[] follows);

        void Stop();

        void AddTrack(string keyword);

        void RemoveTrack(string keyword);
    }
}