using System.ComponentModel.DataAnnotations;
using Autofac;
using Wikiled.Console.Arguments;
using Wikiled.Redis.Config;
using Wikiled.Redis.Logic;
using Wikiled.Twitter.Modules;

namespace Wikiled.ConsoleApp.Commands.Config
{
    public class TwitterLoadConfig : ICommandConfig
    {
        [Required]
        public string Out { get; set; }

        [Required]
        public string TypeName { get; set; }

        public void Build(ContainerBuilder builder)
        {
            builder.RegisterInstance(new RedisLink(TypeName, new RedisMultiplexer(new RedisConfiguration("localhost", 6370))));
            builder.RegisterModule<TwitterModule>();
            builder.RegisterModule(new ConsoleAuthModule());
        }
    }
}
