using System.ComponentModel.DataAnnotations;
using Autofac;
using Wikiled.Console.Arguments;
using Wikiled.Twitter.Modules;

namespace Wikiled.ConsoleApp.Commands.Config
{
    public class DownloadMessagesConfig : ICommandConfig
    {
        [Required]
        public string Ids { get; set; }

        [Required]
        public string Out { get; set; }

        public bool Clean { get; set; }

        public void Build(ContainerBuilder builder)
        {
            builder.RegisterModule<TwitterModule>();
            builder.RegisterModule(new ConsoleAuthModule());
        }
    }
}
