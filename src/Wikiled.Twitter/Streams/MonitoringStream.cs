using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tweetinvi;
using Tweetinvi.Core.Exceptions;
using Tweetinvi.Core.Helpers;
using Tweetinvi.Events;
using Tweetinvi.Models;
using Tweetinvi.Models.DTO;
using Tweetinvi.Streaming;
using Wikiled.Twitter.Security;

namespace Wikiled.Twitter.Streams
{
    public class MonitoringStream : IMonitoringStream
    {
        private readonly ILogger<MonitoringStream> log;

        private readonly IAuthentication auth;

        private readonly HashSet<long> following = new HashSet<long>();

        private int isActive;

        private IFilteredStream stream;

        private long totalReceived;

        private Subject<ITweetDTO> messagesReceiving = new Subject<ITweetDTO>();

        public MonitoringStream(ILogger<MonitoringStream> log, IAuthentication auth)
        {
            this.auth = auth ?? throw new ArgumentNullException(nameof(auth));
            this.log = log ?? throw new ArgumentNullException(nameof(log));
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
            log.LogInformation("Add track {0}", keyword);
            stream.AddTrack(keyword);
        }

        public void RemoveTrack(string keyword)
        {
            log.LogInformation("Remove track {0}", keyword);
            stream.RemoveTrack(keyword);
        }

        public async Task Start(string[] keywords, string[] follows)
        {
            if (keywords == null)
            {
                throw new ArgumentNullException(nameof(keywords));
            }

            log.LogDebug("Starting...");
            IsActive = true;
            ExceptionHandler.SwallowWebExceptions = false;
            ExceptionHandler.WebExceptionReceived += ExceptionHandlerOnWebExceptionReceived;
            var authenticated = auth.Authenticate();
            Auth.SetCredentials(authenticated);
            stream = Stream.CreateFilteredStream(authenticated);
            if (LanguageFilters != null)
            {
                foreach (var filter in LanguageFilters)
                {
                    log.LogInformation("Setting language filter: {0}", filter);
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
                    log.LogError(ex, "Failed to start stream");
                }

                if (IsActive)
                {
                    log.LogInformation("Waiting to retry");
                    await Task.Delay(500).ConfigureAwait(false);
                }
            }
            while (IsActive);
        }

        public void TrackUser(string follow)
        {
            var user = User.GetUserFromScreenName(follow);
            following.Add(user.Id);
            log.LogInformation("Add follow {0}", user);
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
                    log.LogDebug("Message received: [{0}-{3}] - [{1}-{2}]", tweetDto.CreatedBy.Location, tweetDto.CreatedBy.Name, tweetDto.CreatedBy.FollowersCount, tweetDto.Place);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Object processing error: " + jsonObjectEventArgs.Json);
            }
        }

        private void ExceptionHandlerOnWebExceptionReceived(object sender, GenericEventArgs<ITwitterException> genericEventArgs)
        {
            log.LogError(genericEventArgs.Value.WebException, "Web error");
        }

        private void StreamOnWarningFallingBehindDetected(object sender, WarningFallingBehindEventArgs warningFallingBehindEventArgs)
        {
            var message = $"Falling behind {warningFallingBehindEventArgs.WarningMessage}...";
            log.LogWarning(message);
        }

        private void StreamOnStreamStopped(object sender, StreamExceptionEventArgs streamExceptionEventArgs)
        {
            if (streamExceptionEventArgs.Exception != null)
            {
                log.LogError(streamExceptionEventArgs.Exception, "Stream error");
            }

            log.LogInformation("Stream Stopped ({0} {1})...", streamExceptionEventArgs.DisconnectMessage?.Reason, streamExceptionEventArgs.DisconnectMessage?.Code);
        }

        private void StreamOnStreamStarted(object sender, EventArgs eventArgs)
        {
            log.LogInformation("Stream started...");
        }

        private void StreamOnLimitReached(object sender, LimitReachedEventArgs limitReachedEventArgs)
        {
            var message = $"Limit reached: {limitReachedEventArgs.NumberOfTweetsNotReceived}";
            log.LogInformation(message);
        }
    }
}
