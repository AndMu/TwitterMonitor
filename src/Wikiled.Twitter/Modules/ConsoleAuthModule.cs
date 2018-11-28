using System;
using Autofac;
using Tweetinvi.Models;
using Wikiled.Common.Utilities.Config;
using Wikiled.Twitter.Security;

namespace Wikiled.Twitter.Modules
{
    public class ConsoleAuthModule : Module
    {
        private readonly ITwitterCredentials appCredentials;

        public ConsoleAuthModule(ITwitterCredentials appCredentials = null)
        {
            this.appCredentials = appCredentials;
        }
        
        protected override void Load(ContainerBuilder builder)
        {
            if (appCredentials != null)
            {
                builder.RegisterInstance(appCredentials).As<ITwitterCredentials>();
                builder.RegisterType<SimpleCredentialsSource>().As<ICredentialsSource>();
            }
            else
            {
                builder.RegisterType<EnvironmentCredentialsSource>().As<ICredentialsSource>();
            }

            builder.RegisterType<ApplicationConfiguration>().As<IApplicationConfiguration>();
            builder.RegisterType<PinConsoleAuthentication>().Named<IAuthentication>("implementation");
            builder.RegisterType<PersistedAuthentication>().Named<IAuthentication>("decorator");
            builder.RegisterDecorator<IAuthentication>((c, inner) => c.ResolveNamed<IAuthentication>("decorator", TypedParameter.From(inner)), "implementation");
            base.Load(builder);
        }
    }
}
