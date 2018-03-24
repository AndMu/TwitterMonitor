using NUnit.Framework;
using System;
using Wikiled.Twitter.Persistency.Data;

namespace Wikiled.Twitter.Tests.Persistency.Data
{
    [TestFixture]
    public class UserItemTests
    {
        private TweetUser tweetuser;

        private UserItem instance;

        [SetUp]
        public void Setup()
        {
            tweetuser = new TweetUser();
            instance = CreateUserItem();
        }

        [TearDown]
        public void Cleanup()
        {
        }

        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentNullException>(() => new UserItem(null));
            Assert.AreEqual(0, instance.AverageDistance);
        }

        [Test]
        public void AverageDistance()
        {
            Assert.AreEqual(0, instance.AverageDistance);
            instance.Add(new MessageItem(new UserItem(new TweetUser()), new TweetData { Tick = 1, Latitude = 48.237867, Longitude = 16.389477 }));
            Assert.AreEqual(0, instance.AverageDistance);
            instance.Add(
                new MessageItem(
                    new UserItem(new TweetUser()),
                    new TweetData()
                    {
                        Tick = 2,
                        Latitude = 48.672309,
                        Longitude = 15.695585
                    }));
            Assert.AreEqual(35215.795759484099d, instance.AverageDistance);
        }

        private UserItem CreateUserItem()
        {
            return new UserItem(tweetuser);
        }
    }
}
