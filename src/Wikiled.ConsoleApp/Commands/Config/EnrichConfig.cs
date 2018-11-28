using System.ComponentModel.DataAnnotations;
using Autofac;
using Wikiled.Console.Arguments;
using Wikiled.Twitter.Modules;

namespace Wikiled.ConsoleApp.Commands.Config
{
    public class EnrichConfig : ICommandConfig
    {
        [Required]
        public string Topics { get; set; }

        [Required]
        public string Out { get; set; }

        public void Build(ContainerBuilder builder)
        {
            builder.RegisterModule<TwitterModule>();
            builder.RegisterModule(new ConsoleAuthModule());
        }
    }
}
