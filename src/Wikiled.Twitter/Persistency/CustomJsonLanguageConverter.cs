using Newtonsoft.Json;
using System;
using Tweetinvi.Logic.JsonConverters;
using Tweetinvi.Models;

namespace Wikiled.Twitter.Persistency
{
    public class CustomJsonLanguageConverter : JsonLanguageConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value != null
                       ? base.ReadJson(reader, objectType, existingValue, serializer)
                       : Language.English;
        }
    }
}
