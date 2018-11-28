using Autofac;
using Tweetinvi.Models;
using Wikiled.Twitter.Security;

namespace Wikiled.Twitter.Modules
{
    public class ConsoleAuthModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(Credentials.Instance.IphoneTwitterCredentials).As<ITwitterCredentials>();
            builder.RegisterType<PinConsoleAuthentication>().Named<IAuthentication>("implementation");
            builder.RegisterType<PersistedAuthentication>().Named<IAuthentication>("decorator");
            builder.RegisterDecorator<IAuthentication>((c, inner) => c.ResolveNamed<IAuthentication>("decorator", TypedParameter.From(inner)), "implementation");
            base.Load(builder);
        }
    }
}
