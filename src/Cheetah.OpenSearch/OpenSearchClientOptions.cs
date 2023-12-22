using System;
using Newtonsoft.Json;

namespace Cheetah.OpenSearch
{
    public class OpenSearchClientOptions
    {
        public JsonSerializerSettings JsonSerializerSettings { get; private set; } = new JsonSerializerSettings()
        {
            MissingMemberHandling = MissingMemberHandling.Ignore
        };
        
        public bool DisableDirectStreaming { get; set; }

        public OpenSearchClientOptions WithJsonSerializerSettings(Action<JsonSerializerSettings> configure)
        {
            configure(JsonSerializerSettings);
            return this;
        }
    }
}
