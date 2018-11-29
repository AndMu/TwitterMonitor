using System;
using System.Threading.Tasks;
using Tweetinvi.Models;

namespace Wikiled.Twitter.Streams
{
    public interface IUserStream : IDisposable
    {
        IObservable<IMessage> MessageReceived { get; }

        Task Start();

        void Stop();
    }
}