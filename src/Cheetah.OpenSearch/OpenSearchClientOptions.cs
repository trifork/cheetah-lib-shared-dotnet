using System;
using Newtonsoft.Json;
using OpenSearch.Client;

namespace Cheetah.OpenSearch
{
    /// <summary>
    /// Options for configuring the <see cref="OpenSearchClient"/>
    /// </summary>
    public class OpenSearchClientOptions
    {
        /// <summary>
        /// Retrieves the current <see cref="JsonSerializerSettings"/> used by the <see cref="OpenSearchClient"/>
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; } =
            new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Ignore };

        /// <summary>
        /// Retrieves the current <see cref="ConnectionSettings"/> used by the <see cref="OpenSearchClient"/>
        /// </summary>
        internal Action<ConnectionSettings>? ConnectionSettings { get; set; }

        /// <summary>
        /// Configures the <see cref="JsonSerializerSettings"/> used by the <see cref="OpenSearchClient"/>
        /// </summary>
        /// <param name="configure">Action which configures used <see cref="JsonSerializerSettings"/></param>
        /// <returns>This <see cref="OpenSearchClientOptions"/> instance for method chaining.</returns>
        public OpenSearchClientOptions WithJsonSerializerSettings(
            Action<JsonSerializerSettings> configure
        )
        {
            configure(JsonSerializerSettings);
            return this;
        }

        /// <summary>
        /// Configures the <see cref="ConnectionSettings"/> used by the <see cref="OpenSearchClient"/>
        /// </summary>
        /// <param name="configure">Action which configures used <see cref="ConnectionSettings"/></param>
        /// <returns>This <see cref="OpenSearchClientOptions"/> instance for method chaining.</returns>
        public OpenSearchClientOptions WithConnectionSettings(
            Action<ConnectionSettings> configure
        )
        {
            ConnectionSettings = configure;
            return this;
        }
    }
}
