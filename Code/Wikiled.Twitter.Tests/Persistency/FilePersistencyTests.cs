using System;
using System.IO;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Wikiled.Twitter.Persistency;

namespace Wikiled.Twitter.Tests.Persistency
{
    [TestFixture]
    public class FilePersistencyTests
    {
        private FilePersistency instance;

        private Mock<IStreamSource> stream;

        [SetUp]
        public void Setup()
        {
            stream = new Mock<IStreamSource>();
            instance = new FilePersistency(stream.Object);
        }

        [Test]
        public void SaveError()
        {
            stream.Setup(item => item.GetStream()).Throws<NullReferenceException>();
            instance.Save("Test");
        }

        [Test]
        public void Load()
        {
            var result = FilePersistency.Load(Path.Combine(TestContext.CurrentContext.TestDirectory, @"data\data_20160311_1115.dat"));
            Assert.AreEqual(7725, result.Length);
        }
    }
}
