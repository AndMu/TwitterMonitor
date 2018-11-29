using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tweetinvi;
using Wikiled.Twitter.Security;
using IMessage = Tweetinvi.Models.IMessage;

namespace Wikiled.Twitter.Streams
{
    public class UserStream : IUserStream
    {
        private readonly IAuthentication auth;

        private readonly ILogger<UserStream> log;

        private readonly Subject<IMessage> received = new Subject<IMessage>();

        private Tweetinvi.Streaming.IUserStream stream;

        public UserStream(ILogger<UserStream> log, IAuthentication auth)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this.auth = auth ?? throw new ArgumentNullException(nameof(auth));
        }

        public IObservable<IMessage> MessageReceived => received;

        public async Task Start()
        {
            log.LogInformation("Starting user stream");
            var authenticate = auth.Authenticate();
            stream = Stream.CreateUserStream(authenticate);
            await stream.StartStreamAsync().ConfigureAwait(false);
            stream.MessageReceived += (sender, args) =>
            {
                received.OnNext(args.Message);
            };
        }

        public void Stop()
        {
            log.LogInformation("Stop");
            stream?.StopStream();
        }

        public void Dispose()
        {
            log.LogInformation("Dispose");
            received?.Dispose();
            Stop();
        }
    }
}
