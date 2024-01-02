using System;
using Newtonsoft.Json;
using OpenSearch.Client;

namespace Cheetah.OpenSearch
{
    public class OpenSearchClientOptions
    {
        /// <summary>
        /// Retrieves the current <see cref="JsonSerializerSettings"/> used by the <see cref="OpenSearchClient"/>
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; } = new JsonSerializerSettings()
        {
            MissingMemberHandling = MissingMemberHandling.Ignore
        };
        
        /// <summary>
        /// Gets or sets a value indicating whether direct streaming of the response content should be disabled.<br/>
        /// This should <c>false</c> in production environments to avoid buffering of the response content in memory.<br/>
        /// When set to <c>true</c> the response content will be available during debugging, which might not otherwise be the case.
        /// </summary>
        public bool DisableDirectStreaming { get; set; }

        /// <summary>
        /// Configures the <see cref="JsonSerializerSettings"/> used by the <see cref="OpenSearchClient"/>
        /// </summary>
        /// <param name="configure">Action which configures used <see cref="JsonSerializerSettings"/></param>
        /// <returns>This <see cref="OpenSearchClientOptions"/> instance for method chaining.</returns>
        public OpenSearchClientOptions WithJsonSerializerSettings(Action<JsonSerializerSettings> configure)
        {
            configure(JsonSerializerSettings);
            return this;
        }
    }
}
