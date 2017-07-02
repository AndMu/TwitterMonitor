using System.IO;
using System.Xml.Linq;
using NLog;
using Tweetinvi.Models;
using Wikiled.Core.Utility.Arguments;
using Wikiled.Core.Utility.Serialization;

namespace Wikiled.Twitter.Security
{
    public class PersistedAuthentication : IAuthentication
    {
        private readonly IAuthentication underlying;

        private static Logger log = LogManager.GetCurrentClassLogger();

        public PersistedAuthentication(IAuthentication underlying)
        {
            Guard.NotNull(() => underlying, underlying);
            this.underlying = underlying;
        }

        public ITwitterCredentials Authenticate(ITwitterCredentials auth)
        {
            var file = Path.Combine(auth.ConsumerKey, "xml");
            if (File.Exists(file))
            {
                log.Info("Found saved credentials. Loading...");
                return XDocument.Load(file).XmlDeserialize<TwitterCredentials>();
            }

            var credentials = underlying.Authenticate(auth);
            ((TwitterCredentials)credentials).XmlSerialize().Save(Path.Combine(credentials.ConsumerKey, "xml"));
            return credentials;
        }
    }
}
