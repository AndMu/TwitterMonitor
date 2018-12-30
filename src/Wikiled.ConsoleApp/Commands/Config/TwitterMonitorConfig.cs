using System;
using Autofac;
using Wikiled.Console.Arguments;
using Wikiled.Twitter.Modules;
using Wikiled.Twitter.Persistency;

namespace Wikiled.ConsoleApp.Commands.Config
{
    public class TwitterMonitorConfig : ICommandConfig
    {
        public string Out { get; set; }

        public string People { get; set; }

        public bool Compress { get; set; }

        public string Keywords { get; set; }


        public void Build(ContainerBuilder builder)
        {
            builder.RegisterModule<TwitterModule>();
            builder.RegisterModule(new ConsoleAuthModule());
            var path = string.IsNullOrEmpty(Out) ? "out" : Out;
            builder.RegisterInstance(new TimingStreamConfig(path, TimeSpan.FromHours(1)));
            builder.RegisterType<TimingStreamSource>().As<IStreamSource>();
            if (Compress)
            {
                builder.RegisterType<FlatFileSerializer>().As<IPersistency>();
            }
            else
            {
                builder.RegisterType<FilePersistency>().As<IPersistency>();
            }
        }
    }
}
