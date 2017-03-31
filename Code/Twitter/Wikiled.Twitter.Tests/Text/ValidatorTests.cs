using System.Text;
using NUnit.Framework;
using Wikiled.Twitter.Text;

namespace Wikiled.Twitter.Tests.Text
{
    [TestFixture]
    public class ValidatorTests
    {
        private Validator validator;

        [SetUp]
        public void Setup()
        {
            validator = new Validator();
        }

        [Test]
        public void BOMCharacterTest()
        {
            Assert.IsFalse(validator.IsValidTweet("test \uFFFE"));
            Assert.IsFalse(validator.IsValidTweet("test \uFEFF"));
        }

        [Test]
        public void InvalidCharacterTest()
        {
            Assert.IsFalse(validator.IsValidTweet("test \uFFFF"));
            Assert.IsFalse(validator.IsValidTweet("test \uFEFF"));
        }

        [Test]
        public void DirectionChangeCharactersTest()
        {
            Assert.IsFalse(validator.IsValidTweet("test \u202A test"));
            Assert.IsFalse(validator.IsValidTweet("test \u202B test"));
            Assert.IsFalse(validator.IsValidTweet("test \u202C test"));
            Assert.IsFalse(validator.IsValidTweet("test \u202D test"));
            Assert.IsFalse(validator.IsValidTweet("test \u202E test"));
        }

        [Test]
        public void AccentCharactersTest()
        {
            string c = "\u0065\u0301";
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < 139; i++)
            {
                builder.Append(c);
            }
            Assert.IsTrue(validator.IsValidTweet(builder.ToString()));
            Assert.IsTrue(validator.IsValidTweet(builder.Append(c).ToString()));
            Assert.IsFalse(validator.IsValidTweet(builder.Append(c).ToString()));
        }

        [Test]
        public void MutiByteCharactersTest()
        {
            string c = "\ud83d\ude02";
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < 139; i++)
            {
                builder.Append(c);
            }
            Assert.IsTrue(validator.IsValidTweet(builder.ToString()));
            Assert.IsTrue(validator.IsValidTweet(builder.Append(c).ToString()));
            Assert.IsFalse(validator.IsValidTweet(builder.Append(c).ToString()));
        }
    }
}