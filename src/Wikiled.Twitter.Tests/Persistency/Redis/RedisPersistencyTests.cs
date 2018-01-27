using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using NLog;
using NUnit.Framework;
using Tweetinvi;
using Tweetinvi.Core.Helpers;
using Tweetinvi.Logic.DTO;
using Tweetinvi.Models;
using Wikiled.Core.Utility.Cache;
using Wikiled.Redis.Config;
using Wikiled.Redis.Keys;
using Wikiled.Redis.Logic;
using Wikiled.Twitter.Persistency;
using Wikiled.Twitter.Persistency.Data;

namespace Wikiled.Twitter.Tests.Persistency.Redis
{
    [TestFixture]
    public class RedisPersistencyTests
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        private ITweet tweet;

        private RedisPersistency persistency;

        private RedisLink link;

        private RedisInside.Redis redis;

        private MemoryCache cache;

        [SetUp]
        public void Setup()
        {
            redis = new RedisInside.Redis(i => i.Port(6666).LogTo(message => log.Debug(message)));
            var config = new RedisConfiguration("localhost", 6666);
            var jsonConvert = TweetinviContainer.Resolve<IJsonObjectConverter>();
            var jsons =
                FilePersistency.Load(Path.Combine(TestContext.CurrentContext.TestDirectory, @"data\data_20160311_1115.dat"));
            var tweetDto = jsonConvert.DeserializeObject<TweetDTO>(jsons[0]);
            tweet = Tweet.GenerateTweetFromDTO(tweetDto);
            link = new RedisLink("Trump", new RedisMultiplexer(config));
            link.Open();
            cache = new MemoryCache("Redis");
            persistency = new RedisPersistency(link, new RuntimeCache(cache, TimeSpan.FromMinutes(1)));
            persistency.ResolveRetweets = true;
        }

        [TearDown]
        public void Clean()
        {
            link.Dispose();
            cache.Dispose();
            redis.Dispose();
        }

        [Test]
        public async Task BasicOperations()
        {
            var expected = await persistency.Count("All").ConfigureAwait(false);
            Assert.AreEqual(0, expected);

            expected = await persistency.CountUser().ConfigureAwait(false);
            Assert.AreEqual(0, expected);

            await persistency.Save(tweet).ConfigureAwait(false);

            expected = await persistency.Count("All").ConfigureAwait(false);
            Assert.AreEqual(2, expected, "Saved with retweet");

            expected = await persistency.CountUser().ConfigureAwait(false);
            Assert.AreEqual(2, expected);
        }

        [Test]
        public async Task TestNullUser()
        {
            await persistency.Save(tweet).ConfigureAwait(false);
            var user = await persistency.LoadUser(tweet.CreatedBy.Id).ConfigureAwait(false);
            MemoryCache.Default.Remove($"User{user.User.Id}");
            MemoryCache.Default.Remove($"{user.User.Id}");
            string[] key = { "User", user.User.Id.ToString() };
            var repKey = new RepositoryKey(persistency, new ObjectKey(key));
            await link.Client.DeleteAll<TweetUser>(repKey).ConfigureAwait(false);
            var message = persistency.LoadAll().Catch(Observable.Empty<MessageItem>()).ToEnumerable().ToArray();
        }

        [Test]
        public async Task TestUpdateUser()
        {
            await persistency.Save(tweet).ConfigureAwait(false);
            var result = persistency.LoadAll().ToEnumerable().ToArray();
            var user = await persistency.LoadUser(result[0].User.User.Id).ConfigureAwait(false);
            Assert.IsNotNull(user);
            Assert.AreEqual(1, user.Messages.Length);

            await persistency.SaveUser(user.User).ConfigureAwait(false);
            var user2 = await persistency.LoadUser(result[0].User.User.Id).ConfigureAwait(false);
            Assert.AreNotSame(user, user2);
            Assert.AreEqual(0, user2.Messages.Length);
            result = persistency.LoadAll().ToEnumerable().ToArray();
            user2 = await persistency.LoadUser(result[0].User.User.Id).ConfigureAwait(false);
            Assert.AreEqual(1, user2.Messages.Length);
            Assert.AreNotSame(user.Messages[0], user2.Messages[0]);
        }

        [Test]
        public async Task TestLoadAll()
        {
            await persistency.Save(tweet).ConfigureAwait(false);
            var result = persistency.LoadAll().ToEnumerable().ToArray();
            Assert.AreEqual(2, result.Length);
            Assert.AreSame(result[0], result[0].User.Messages[0]);
            if (result[1].Data.IsRetweet)
            {
                Assert.AreSame(result[0], result[1].Retweet);
            }

            if (result[0].Data.IsRetweet)
            {
                Assert.AreSame(result[1], result[0].Retweet);
            }

            Assert.IsNotEmpty(result[0].Data.Text);
        }

        [Test]
        public async Task TestDublicateSave()
        {
            await persistency.Save(tweet).ConfigureAwait(false);
            await persistency.Save(tweet).ConfigureAwait(false);
            var expected = await persistency.Count("All").ConfigureAwait(false);
            Assert.AreEqual(2, expected, "Saved with retweet");

            expected = await persistency.CountUser().ConfigureAwait(false);
            Assert.AreEqual(2, expected);
        }

        [Test]
        public async Task TestSimpleCache()
        {
            await persistency.Save(tweet).ConfigureAwait(false);
            var result = await persistency.LoadMessage(tweet.Id).ConfigureAwait(false);
            var result2 = await persistency.LoadMessage(tweet.Id).ConfigureAwait(false);
            Assert.AreSame(result, result2);
        }

        [Test]
        public async Task TestLoad()
        {
            await persistency.Save(tweet).ConfigureAwait(false);
            var result = await persistency.LoadMessage(tweet.Id).ConfigureAwait(false);
            Assert.IsNotNull(result);
            Assert.AreEqual(3318421381, result.CreatorId);
            Assert.AreEqual(708168747324825601, result.RetweetedId);
        }
    }
}
