using Autofac;
using Tweetinvi;
using Wikiled.Twitter.Discovery;
using Wikiled.Twitter.Streams;

namespace Wikiled.Twitter.Modules
{
    public class TwitterModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            RateLimit.RateLimitTrackerMode = RateLimitTrackerMode.TrackAndAwait;
            builder.RegisterType<MessageDiscovery>().As<IMessageDiscovery>();
            builder.RegisterType<MonitoringStream>().As<IMonitoringStream>();
            base.Load(builder);
        }
    }
}
