using NUnit.Framework;
using Wikiled.Testing.Library.Processes;

namespace Wikiled.Twitter.Tests.Persistency.Redis
{
    [SetUpFixture]
    public class Global
    {
        private RedisProcessManager manager;

        [OneTimeSetUp]
        public void Setup()
        {
            manager = new RedisProcessManager();
            manager.Start();
        }

        [OneTimeTearDown]
        public void Clean()
        {
            manager.Dispose();
        }
    }
}
