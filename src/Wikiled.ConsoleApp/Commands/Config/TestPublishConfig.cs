using System.ComponentModel.DataAnnotations;
using Autofac;
using Wikiled.Console.Arguments;
using Wikiled.Twitter.Modules;

namespace Wikiled.ConsoleApp.Commands.Config
{
    public class TestPublishConfig : ICommandConfig
    {
        public string Image { get; set; }

        [Required]
        public string Text { get; set; }

        public void Build(ContainerBuilder builder)
        {
            builder.RegisterModule<TwitterModule>();
            builder.RegisterModule(new ConsoleAuthModule());
        }
    }
}
