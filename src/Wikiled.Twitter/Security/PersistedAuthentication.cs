using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using Tweetinvi.Models;

namespace Wikiled.Twitter.Security
{
    public class PersistedAuthentication : IAuthentication
    {
        private readonly ILogger<PersistedAuthentication> log;

        private readonly IAuthentication underlying;

        public PersistedAuthentication(ILogger<PersistedAuthentication> log, IAuthentication underlying)
        {
            this.underlying = underlying ?? throw new ArgumentNullException(nameof(underlying));
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public ITwitterCredentials Authenticate()
        {
            var file = "key.auth";
            string json;
            if (File.Exists(file))
            {
                log.LogInformation("Found saved applicationCredentials. Loading...");
                json = File.ReadAllText(file);
                return JsonConvert.DeserializeObject<TwitterCredentials>(json);
            }

            var credentials = underlying.Authenticate();
            json = JsonConvert.SerializeObject((TwitterCredentials)credentials);
            string jsonFormatted = JToken.Parse(json).ToString(Formatting.Indented);
            File.WriteAllText(file, jsonFormatted);
            return credentials;
        }
    }
}
