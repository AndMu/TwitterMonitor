using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Tweetinvi;
using Tweetinvi.Core.Exceptions;
using Tweetinvi.Core.Helpers;
using Tweetinvi.Events;
using Tweetinvi.Models;
using Tweetinvi.Models.DTO;
using Tweetinvi.Streaming;
using Wikiled.Common.Arguments;
using Wikiled.Twitter.Security;

namespace Wikiled.Twitter.Streams
{
    public class MonitoringStream : IMonitoringStream
    {
        private readonly IAuthentication auth;

        private readonly HashSet<long> following = new HashSet<long>();

        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private int isActive;

        private IFilteredStream stream;

        private long totalReceived;

        private Subject<ITweetDTO> messagesReceiving = new Subject<ITweetDTO>();

        public MonitoringStream(IAuthentication auth)
        {
            Guard.NotNull(() => auth, auth);
            this.auth = auth;
        }

        public IObservable<ITweetDTO> MessagesReceiving => messagesReceiving;

        public bool IsActive { get => Interlocked.CompareExchange(ref isActive, 0, 0) == 1; private set => Interlocked.Exchange(ref isActive, value ? 1 : 0); }

        public LanguageFilter[] LanguageFilters { get; set; }

        public long TotalReceived => Interlocked.Read(ref totalReceived);

        public void Dispose()
        {
            Stop();
            messagesReceiving?.OnCompleted();
            messagesReceiving?.Dispose();
            messagesReceiving = null;
        }

        public void AddTrack(string keyword)
        {
            log.Info("Add track {0}", keyword);
            stream.AddTrack(keyword);
        }

        public void RemoveTrack(string keyword)
        {
            log.Info("Remove track {0}", keyword);
            stream.RemoveTrack(keyword);
        }

        public async Task Start(string[] keywords, string[] follows)
        {
            Guard.NotNull(() => keywords, keywords);
            log.Debug("Starting...");
            IsActive = true;
            ExceptionHandler.SwallowWebExceptions = false;
            ExceptionHandler.WebExceptionReceived += ExceptionHandlerOnWebExceptionReceived;

            stream = Stream.CreateFilteredStream(auth.Authenticate());
            if (LanguageFilters != null)
            {
                foreach (var filter in LanguageFilters)
                {
                    log.Info("Setting language filter: {0}", filter);
                    stream.AddTweetLanguageFilter(filter);
                }
            }

            stream.JsonObjectReceived += StreamOnJsonObjectReceived;
            foreach (var keyword in keywords)
            {
                AddTrack(keyword);
            }

            if (follows != null)
            {
                foreach (var follow in follows)
                {
                    TrackUser(follow);
                }
            }

            stream.StallWarnings = true;
            stream.LimitReached += StreamOnLimitReached;
            stream.StreamStarted += StreamOnStreamStarted;
            stream.StreamStopped += StreamOnStreamStopped;
            stream.WarningFallingBehindDetected += StreamOnWarningFallingBehindDetected;
            do
            {
                try
                {
                    await stream.StartStreamMatchingAnyConditionAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    log.Error(ex, "Failed to start stream");
                }

                if (IsActive)
                {
                    log.Info("Waiting to retry");
                    await Task.Delay(500).ConfigureAwait(false);
                }
            }
            while (IsActive);
        }

        public void TrackUser(string follow)
        {
            IUser user = User.GetUserFromScreenName(follow);
            following.Add(user.Id);
            log.Info("Add follow {0}", user);
            stream.AddFollow(user);
        }

        public void Stop()
        {
            if (!IsActive)
            {
                return;
            }

            IsActive = false;
            stream?.StopStream();
        }

        private void StreamOnJsonObjectReceived(object sender, JsonObjectEventArgs jsonObjectEventArgs)
        {
            try
            {
                Interlocked.Increment(ref totalReceived);
                var json = jsonObjectEventArgs.Json;
                var jsonConvert = TweetinviContainer.Resolve<IJsonObjectConverter>();
                var tweetDto = jsonConvert.DeserializeObject<ITweetDTO>(json);
                messagesReceiving.OnNext(tweetDto);
                if (tweetDto.CreatedBy != null)
                {
                    log.Debug("Message received: [{0}-{3}] - [{1}-{2}]", tweetDto.CreatedBy.Location, tweetDto.CreatedBy.Name, tweetDto.CreatedBy.FollowersCount, tweetDto.Place);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Object received");
            }
        }

        private void ExceptionHandlerOnWebExceptionReceived(object sender, GenericEventArgs<ITwitterException> genericEventArgs)
        {
            log.Error(genericEventArgs.Value.WebException, "Web error");
        }

        private void StreamOnWarningFallingBehindDetected(object sender, WarningFallingBehindEventArgs warningFallingBehindEventArgs)
        {
            string message = $"Falling behind {warningFallingBehindEventArgs.WarningMessage}...";
            log.Warn(message);
        }

        private void StreamOnStreamStopped(object sender, StreamExceptionEventArgs streamExceptionEventArgs)
        {
            log.Info("Stream Stopped...");
        }

        private void StreamOnStreamStarted(object sender, EventArgs eventArgs)
        {
            log.Info("Stream started...");
        }

        private void StreamOnLimitReached(object sender, LimitReachedEventArgs limitReachedEventArgs)
        {
            string message = $"Limit reached: {limitReachedEventArgs.NumberOfTweetsNotReceived}";
            log.Info(message);
        }
    }
}
