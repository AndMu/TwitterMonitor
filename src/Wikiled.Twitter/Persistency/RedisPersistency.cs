using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Tweetinvi.Models;
using Wikiled.Redis.Indexing;
using Wikiled.Redis.Keys;
using Wikiled.Redis.Logic;
using Wikiled.Text.Analysis.Twitter;
using Wikiled.Twitter.Persistency.Data;

namespace Wikiled.Twitter.Persistency
{
    public class RedisPersistency : IRedisPersistency
    {
        private readonly ILogger<RedisPersistency> log;

        private const string AllTweets = "All";

        private const string AllUserTag = "AllUsers";

        private const string CoordinatesTag = "Coordinates";

        private const string GeoTag = "Geo";

        private const string Original = "Original";

        private const string PlaceTag = "Place";

        private const string TweetTag = "Tweet";

        private const string UserLocationTag = "UserLocation";

        private const string UserTag = "User";

        private readonly IMemoryCache cache;

        private readonly Extractor extractor = new Extractor();

        private readonly IRedisLink redis;

        public RedisPersistency(ILogger<RedisPersistency> log, IRedisLink redis, IMemoryCache cache)
        {
            if (redis == null)
            {
                throw new ArgumentNullException(nameof(redis));
            }

            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            redis.RegisterHashType<TweetData>().IsSingleInstance = true;
            redis.RegisterHashType<TweetUser>().IsSingleInstance = true;
            this.redis = redis;
        }

        public string Name => "Twitter";

        public bool ResolveRetweets { get; set; }

        public Task<long> Count(string keyName)
        {
            var key = new IndexKey(this, keyName, false);
            return new IndexManagerFactory(redis, redis.Database).Create(key).Count();
        }

        public Task<long> CountUser()
        {
            return Count(AllUserTag);
        }

        public IObservable<MessageItem> LoadAll(string index = AllTweets, long begin = 0, long end = -1)
        {
            var key = new IndexKey(this, index, false);
            var observable = redis.Client.GetRecords<TweetData>(key, begin, end);
            return observable.SelectMany(
                                 item => Observable.Start(
                                     async () =>
                                         {
                                             var message = await ConstructMessage(item).ConfigureAwait(false);
                                             if (message.Data.IsRetweet && ResolveRetweets)
                                             {
                                                 string id = $"Message{message.Data.RetweetedId}";
                                                 MessageItem retweet = await cache.GetOrCreateAsync(
                                                     id,
                                                     async cacheItem =>
                                                         {
                                                             cacheItem.SlidingExpiration = TimeSpan.FromMinutes(1);
                                                             var retweetData = await LoadMessage(message.Data.RetweetedId).ConfigureAwait(false);
                                                             return await ConstructMessage(retweetData).ConfigureAwait(false);
                                                         }).ConfigureAwait(false);

                                                 message.Retweet = retweet;
                                             }

                                             return message;
                                         }))
                             .Merge();
        }

        public IObservable<UserItem> LoadAllUsers(int begin = 0, int end = -1)
        {
            var key = new IndexKey(this, AllUserTag, false);
            var observable = redis.Client.GetRecords<TweetUser>(key, begin, end);
            return observable.Select(
                item =>
                    {
                        var userItem = new UserItem(item);
                        return cache.GetOrCreate(item.Id.ToString(), cacheEntry => userItem);
                    });
        }

        public async Task<TweetData> LoadMessage(long id)
        {
            var key = GetTweetKey(id);
            string idText = id.ToString();
            if (cache.TryGetValue(idText, out TweetData message))
            {
                return message;
            }

            message = await redis.Client.GetRecords<TweetData>(key).FirstOrDefaultAsync();
            return cache.GetOrCreate(
                idText,
                cacheEntry =>
                    {
                        cacheEntry.SlidingExpiration = TimeSpan.FromMinutes(1);
                        return message;
                    });
        }

        public async Task<UserItem> LoadUser(long id)
        {
            string idText = $"User{id}";
            if (cache.TryGetValue(idText, out UserItem user))
            {
                return user;
            }

            var key = GetUserKey(id);
            var userItem = await redis.Client.GetRecords<TweetUser>(key).LastAsync();
            user = new UserItem(userItem);
            return cache.GetOrCreate(
                idText,
                cacheEntry =>
                    {
                        cacheEntry.SlidingExpiration = TimeSpan.FromMinutes(1);
                        return user;
                    });
        }

        public async Task Save(ITweet tweet)
        {
            if (tweet == null)
            {
                throw new ArgumentNullException(nameof(tweet));
            }

            var idText = tweet.Id.ToString();
            if (cache.TryGetValue(idText, out TweetData data))
            {
                return;
            }

            data = new TweetData();
            var cached = cache.GetOrCreate(
                idText,
                cacheEntry =>
                    {
                        cacheEntry.SlidingExpiration = TimeSpan.FromMinutes(1);
                        return data;
                    });
            if (cached != data)
            {
                return;
            }

            var key = GetTweetKey(tweet.Id);
            data.Id = tweet.Id;
            key.AddIndex(new IndexKey(this, AllTweets, false));
            if (tweet.Coordinates != null)
            {
                data.Longitude = tweet.Coordinates.Longitude;
                data.Latitude = tweet.Coordinates.Latitude;
                key.AddIndex(new IndexKey(this, CoordinatesTag, false));
            }

            if (tweet.Place != null)
            {
                data.Country = tweet.Place.Country;
                data.CountryCode = tweet.Place.CountryCode;
                data.PlaceType = tweet.Place.PlaceType.ToString();
                data.Place = tweet.Place.FullName;
                key.AddIndex(new IndexKey(this, PlaceTag, false));
                if (!string.IsNullOrEmpty(tweet.Place.Country))
                {
                    key.AddIndex(new IndexKey(this, $"{PlaceTag}:{tweet.Place.Country}", false));
                }

                key.AddIndex(new IndexKey(this, $"{PlaceTag}:{tweet.Place.PlaceType}", false));
            }

            if (tweet.CreatedBy != null)
            {
                await SaveUser(tweet.CreatedBy).ConfigureAwait(false);
                data.Creator = tweet.CreatedBy.Name;
                data.CreatorId = tweet.CreatedBy.Id;
                key.AddIndex(new IndexKey(this, $"{UserTag}:{tweet.CreatedBy.Id}", false));
                data.UserLocation = tweet.CreatedBy.Location;
                if (tweet.CreatedBy.GeoEnabled)
                {
                    key.AddIndex(new IndexKey(this, GeoTag, false));
                }

                if (!string.IsNullOrEmpty(tweet.CreatedBy.Location))
                {
                    key.AddIndex(new IndexKey(this, UserLocationTag, false));
                }
            }

            data.Created = tweet.CreatedAt.ToString();
            data.Tick = tweet.CreatedAt.Ticks;
            data.IsRetweet = tweet.IsRetweet;
            if (!data.IsRetweet)
            {
                key.AddIndex(new IndexKey(this, Original, false));
                data.Text = tweet.Text;
                if (!string.IsNullOrEmpty(tweet.Text))
                {
                    var tags = extractor.ExtractHashtags(tweet.Text);
                    foreach (var tag in tags.Distinct())
                    {
                        key.AddIndex(new IndexKey(this, $"{TweetTag}:{tag.ToLower()}", false));
                    }
                }
            }
            else
            {
                if (tweet.Retweets != null)
                {
                    foreach (var retweet in tweet.Retweets)
                    {
                        data.RetweetedId = tweet.RetweetedTweet.Id;
                        await Save(retweet).ConfigureAwait(false);
                    }
                }

                if (tweet.RetweetedTweet != null)
                {
                    data.RetweetedId = tweet.RetweetedTweet.Id;
                    await Save(tweet.RetweetedTweet).ConfigureAwait(false);
                }
            }

            data.RetweetCount = tweet.RetweetCount;
            var contains = await redis.Client.ContainsRecord<TweetUser>(key).ConfigureAwait(false);
            if (!contains)
            {
                await redis.Client.AddRecord(key, data).ConfigureAwait(false);
            }
        }

        public async Task SaveUser(TweetUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var key = GetUserKey(user.Id);

            var contains = await redis.Client.ContainsRecord<TweetUser>(key).ConfigureAwait(false);
            if (!contains)
            {
                log.LogDebug("User doesn't exist - creating new");
                key.AddIndex(new IndexKey(this, AllUserTag, false));
            }
            else
            {
                InvalidateUser(user.Id);
            }

            await redis.Client.AddRecord(key, user).ConfigureAwait(false);
            cache.Set(user.Id.ToString(), user, new MemoryCacheEntryOptions() { SlidingExpiration = TimeSpan.FromMinutes(1) });
        }

        public async Task SaveUser(IUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var idText = user.Id.ToString();
            if (cache.TryGetValue(idText, out TweetUser data))
            {
                return;
            }

            data = new TweetUser();
            var cached = cache.GetOrCreate(
                idText,
                cacheEntry =>
                    {
                        cacheEntry.SlidingExpiration = TimeSpan.FromMinutes(1);
                        return data;
                    });
            if (cached != data)
            {
                return;
            }

            data.Location = user.Location;
            data.CreatedAt = user.CreatedAt;
            data.DefaultProfile = user.DefaultProfile;
            data.Description = user.Description;

            data.GeoEnabled = user.GeoEnabled;
            data.Url = user.Url;
            data.Language = user.Language.ToString();
            data.FollowersCount = user.FollowersCount;
            data.Following = user.Following;
            data.FriendsCount = user.FriendsCount;
            data.Verified = user.Verified;
            data.Notifications = user.Notifications;
            data.FollowRequestSent = user.FollowRequestSent;
            data.ProfileImageUrl = user.ProfileImageUrl;
            data.ListedCount = user.ListedCount;
            data.UtcOffset = user.UtcOffset;
            data.TimeZone = user.TimeZone;
            data.Name = user.Name;
            data.Protected = user.Protected;
            data.Id = user.Id;
            data.StatusesCount = user.StatusesCount;

            var key = GetUserKey(user.Id);
            key.AddIndex(new IndexKey(this, AllUserTag, false));

            var contains = await redis.Client.ContainsRecord<TweetUser>(key).ConfigureAwait(false);
            if (!contains)
            {
                await redis.Client.AddRecord(key, data).ConfigureAwait(false);
            }
        }

        private async Task<MessageItem> ConstructMessage(TweetData item)
        {
            var mainId = $"Message{item.Id}";
            var user = await LoadUser(item.CreatorId).ConfigureAwait(false);
            var message = new MessageItem(user, item);
            var added = cache.GetOrCreate(
                mainId,
                cacheEntry =>
                    {
                        cacheEntry.SlidingExpiration = TimeSpan.FromMinutes(1);
                        return message;
                    });
            if (added == message)
            {
                added.User.Add(added);
            }

            return message;
        }

        private IDataKey GetTweetKey(long id)
        {
            string[] key = { "Tweet", id.ToString() };
            return new RepositoryKey(this, new ObjectKey(key));
        }

        private IDataKey GetUserKey(long id)
        {
            string[] key = { UserTag, id.ToString() };
            return new RepositoryKey(this, new ObjectKey(key));
        }

        private void InvalidateUser(long id)
        {
            string idText = $"User{id}";
            if (cache.TryGetValue(idText, out UserItem user))
            {
                cache.Remove(idText);
                foreach (var userMessage in user.Messages)
                {
                    string idMessage = $"Message{userMessage.Data.Id}";
                    cache.Remove(idMessage);
                }
            }
        }
    }
}
