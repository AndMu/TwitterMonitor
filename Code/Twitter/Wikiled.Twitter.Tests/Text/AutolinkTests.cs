using System.Collections.Generic;
using NUnit.Framework;
using Wikiled.Twitter.Text;

namespace Wikiled.Twitter.Tests.Text
{

    [TestFixture]
    public class AutolinkTests
    {
        private Autolink instance;

        [SetUp]
        public void Setup()
        {
            instance = new Autolink {NoFollow = false};
        }

        [Test]
        public void NoFollowByDefault()
        {
            var autolink = new Autolink();
            string tweet = "This has a #hashtag";
            string expected = "This has a <a href=\"https://twitter.com/#!/search?q=%23hashtag\" title=\"#hashtag\" class=\"tweet-url hashtag\" rel=\"nofollow\">#hashtag</a>";
            string actual = autolink.AutoLinkHashtags(tweet);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void NoFollowDisabled()
        {
            string tweet = "This has a #hashtag";
            string expected = "This has a <a href=\"https://twitter.com/#!/search?q=%23hashtag\" title=\"#hashtag\" class=\"tweet-url hashtag\">#hashtag</a>";
            string actual = instance.AutoLinkHashtags(tweet);
            Assert.AreEqual(expected, actual);
        }

        /** See Also: http://github.com/mzsanford/twitter-text-rb/issues#issue/5 */

        [Test]
        public void BlogspotWithDashTest()
        {
            string tweet = "Url: http://samsoum-us.blogspot.com/2010/05/la-censure-nuit-limage-de-notre-pays.html";
            string expected =
                "Url: <a href=\"http://samsoum-us.blogspot.com/2010/05/la-censure-nuit-limage-de-notre-pays.html\">http://samsoum-us.blogspot.com/2010/05/la-censure-nuit-limage-de-notre-pays.html</a>";
            string actual = instance.AutoLinkUrls(tweet);
            Assert.AreEqual(expected, actual);
        }

        /** See also: https://github.com/mzsanford/twitter-text-java/issues/8 */

        [Test]
        public void URLWithDollarThatLooksLikeARegexTest()
        {
            string tweet = "Url: http://example.com/$ABC";
            string expected = "Url: <a href=\"http://example.com/$ABC\">http://example.com/$ABC</a>";
            string actual = instance.AutoLinkUrls(tweet);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void URLWithoutProtocolTest()
        {
            string tweet = "Url: www.twitter.com http://www.twitter.com";
            string expected = "Url: www.twitter.com <a href=\"http://www.twitter.com\">http://www.twitter.com</a>";
            string actual = instance.AutoLinkUrls(tweet);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void URLEntitiesTest()
        {
            var autolink = new Autolink();
            var entity = new TweetEntity(0, 19, "http://t.co/0JG5Mcq", TweetEntityType.Url);
            entity.DisplayUrl = "blog.twitter.com/2011/05/twitte…";
            entity.ExpandedUrl = "http://blog.twitter.com/2011/05/twitter-for-mac-update.html";
            var entities = new List<TweetEntity>();
            entities.Add(entity);
            string tweet = "http://t.co/0JG5Mcq";
            string expected =
                "<a href=\"http://t.co/0JG5Mcq\" title=\"http://blog.twitter.com/2011/05/twitter-for-mac-update.html\" rel=\"nofollow\"><span class='tco-ellipsis'><span style='position:absolute;left:-9999px;'>&nbsp;</span></span><span style='position:absolute;left:-9999px;'>http://</span><span class='js-display-url'>blog.twitter.com/2011/05/twitte</span><span style='position:absolute;left:-9999px;'>r-for-mac-update.html</span><span class='tco-ellipsis'><span style='position:absolute;left:-9999px;'>&nbsp;</span>…</span></a>";
            var actual = autolink.AutoLinkEntities(tweet, entities.ToArray());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void WithAngleBracketsTest()
        {
            string tweet = "(Debugging) <3 #idol2011";
            string expected = "(Debugging) &lt;3 <a href=\"https://twitter.com/#!/search?q=%23idol2011\" title=\"#idol2011\" class=\"tweet-url hashtag\">#idol2011</a>";
            string actual = instance.AutoLink(tweet);
            Assert.AreEqual(expected, actual);

            tweet = "<link rel='true'>http://example.com</link>";
            expected = "<link rel='true'><a href=\"http://example.com\">http://example.com</a></link>";
            actual = instance.AutoLinkUrls(tweet);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void UsernameIncludeSymbolTest()
        {
            var autolink = new Autolink {NoFollow = true, UsernameIncludeSymbol = true};
            string tweet = "Testing @mention and @mention/list";
            string expected =
                "Testing <a class=\"tweet-url username\" href=\"https://twitter.com/mention\" rel=\"nofollow\">@mention</a> and <a class=\"tweet-url list-slug\" href=\"https://twitter.com/mention/list\" rel=\"nofollow\">@mention/list</a>";
            string actual = autolink.AutoLink(tweet);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void UrlClassTest()
        {
            string tweet = "http://twitter.com";
            string expected = "<a href=\"http://twitter.com\">http://twitter.com</a>";
            string actual = this.instance.AutoLink(tweet);
            Assert.AreEqual(expected, actual);

            var autolink = new Autolink {UrlClass = "testClass", NoFollow = false};
            autolink.UrlClass = "testClass";
            expected = "<a href=\"http://twitter.com\" class=\"testClass\">http://twitter.com</a>";
            actual = autolink.AutoLink(tweet);
            Assert.AreEqual(expected, actual);

            tweet = "#hash @tw";
            string result = this.instance.AutoLink(tweet);
            Assert.IsTrue(result.Contains("class=\"" + this.instance.HashtagClass + "\""));
            Assert.IsTrue(result.Contains("class=\"" + this.instance.UsernameClass + "\""));
            Assert.IsFalse(result.Contains("class=\"testClass\""));
        }

        [Test]
        public void SymbolTagTest()
        {
            var autolink = new Autolink {SymbolTag = "s", TextWithSymbolTag = "b", NoFollow = false};

            string tweet = "#hash";
            string expected = "<a href=\"https://twitter.com/#!/search?q=%23hash\" title=\"#hash\" class=\"tweet-url hashtag\"><s>#</s><b>hash</b></a>";
            string actual = autolink.AutoLink(tweet);
            Assert.AreEqual(expected, actual);

            tweet = "@mention";
            expected = "<s>@</s><a class=\"tweet-url username\" href=\"https://twitter.com/mention\"><b>mention</b></a>";
            actual = autolink.AutoLink(tweet);
            Assert.AreEqual(expected, actual);

            autolink.UsernameIncludeSymbol = true;
            expected = "<a class=\"tweet-url username\" href=\"https://twitter.com/mention\"><s>@</s><b>mention</b></a>";
            actual = autolink.AutoLink(tweet);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void UrlTargetTest()
        {
            var autolink = new Autolink {UrlTarget = "_blank", NoFollow = false};

            string tweet = "http://test.com";
            string expected = "<a href=\"http://test.com\" target=\"_blank\">http://test.com</a>";
            string actual = autolink.AutoLink(tweet);
            Assert.AreEqual(expected, actual);
        }
    }
}