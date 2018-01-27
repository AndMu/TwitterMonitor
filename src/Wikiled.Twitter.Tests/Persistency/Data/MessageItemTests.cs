using NUnit.Framework;
using Wikiled.Twitter.Persistency.Data;

namespace Wikiled.Twitter.Tests.Persistency.Data
{
    [TestFixture]
    public class MessageItemTests
    {
        private MessageItem instance;

        [SetUp]
        public void Setup()
        {
            instance = CreateTweetData();
        }

        [Test]
        public void Construct()
        {
            Assert.IsNotNull(instance.User);
            Assert.AreEqual(0, instance.CalculateDistance(instance));
        }

        private MessageItem CreateTweetData()
        {
            return new MessageItem(new UserItem(new TweetUser()), new TweetData() { Longitude = 10, Latitude = 10 });
        }
    }
}
