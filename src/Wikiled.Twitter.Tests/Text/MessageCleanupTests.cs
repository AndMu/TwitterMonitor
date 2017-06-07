using System.Threading.Tasks;
using NUnit.Framework;
using Wikiled.Twitter.Persistency.Data;
using Wikiled.Twitter.Text;

namespace Wikiled.Twitter.Tests.Text
{
    [TestFixture]
    public class MessageCleanupTests
    {
        private MessageCleanup instance;

        [SetUp]
        public void Setup()
        {
            instance = CreateMessageCleanup();
        }

        [TestCase(@"Hi http://www.wikiled.com trump", "hi trump")]
        [TestCase(@"Hi @mister trump", "hi AT_USER trump")]
        [TestCase(@"Hi @mister!!! trump????", "hi AT_USER! trump?")]
        [TestCase(@"Hiiii suuuperrrr TRRRump", "hii suuperr trrump")]
        [TestCase(@"@realDonaldTrump @seanhannity WAR ZONE IN N.C.❗TRUMP - @rudygiulianiGOP & amp; @PaulBabeuAZ R THE BEST 4 PROTECTING FOLKS❗NEED ALL OF U IN DC NOW❗",
                    "@realdonaldtrump AT_USER war zone in n.c. :exclamation: trump - AT_USER & amp; AT_USER r the best 4 protecting folks :exclamation: need all of u in dc now :exclamation:")]
        [TestCase(@"Did @realDonaldTrump just claim to speak for all black people? Even while applauding #StopandFrisk ? #STFUDonny #Debate2016",
                    "did @realdonaldtrump just claim to speak for all black people? even while applauding #stopandfrisk ? #stfudonny #debate2016")]
        [TestCase(@"Donald Trump Sighting: Cleveland, Ohio/ New Spirit Revival Center https://t.co/a762lFPC6T @realDonaldTrump", "donald trump sighting: cleveland, ohio/ new spirit revival center @realdonaldtrump")]
        [TestCase(@"up 💪🙏👊🏻⚖ donald j.trump for president 2016", "up :muscle: :pray: :facepunch: :skin-tone-2: :scales: donald j.trump for president 2016")]
        [TestCase(@"⚖#⃣💪#⃣", ":scales: :hash: :muscle: :hash:")]
        [TestCase(@"#melaniatrump campaigning for anti-bullying", "#melaniatrump campaigning for anti-bullying")]
        [TestCase(@"���� Democracy inTRUMPtion ���� #trump #usa #notmypresident #urlo #munch @ Via Roma Cuneo https://t.co/DJ1K1TQnt4", "� democracy intrumption � #trump #usa #notmypresident #urlo #munch @ via roma cuneo")]
        public async Task Cleanup(string message, string expected)
        {
            var result = await instance.Cleanup(new TweetData {Text = message});
            Assert.AreEqual(expected, result);
        }

        private MessageCleanup CreateMessageCleanup()
        {
            return new MessageCleanup();
        }
    }
}
