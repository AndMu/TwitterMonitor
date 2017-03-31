using System;
using System.Linq;
using NUnit.Framework;
using Wikiled.Core.Utility.Extensions;
using Wikiled.Twitter.EmojiSharp;

namespace Wikiled.Twitter.Tests.EmojiSharp
{
    [TestFixture]
    public class EmojiSentimentTests
    {
        [Test]
        public void Generate()
        {
            var positive = EmojiSentiment.GetPositive().Distinct().Select(item => $"EMOTICON_{item.AsShortcode()}\t2").AccumulateItems(Environment.NewLine);
            var negative = EmojiSentiment.GetNegative().Distinct().Select(item => $"EMOTICON_{item.AsShortcode()}\t-2").AccumulateItems(Environment.NewLine);
            Assert.IsNotNull(positive);
            Assert.IsNotNull(negative);
        }
    }
}
