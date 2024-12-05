using System;
using System.Text.Json;
using System.Text.Json.Serialization;
// using Newtonsoft.Json;
using OpenSearch.Client;

namespace Cheetah.OpenSearch
{
    /// <summary>
    /// Options for configuring the <see cref="OpenSearchClient"/>
    /// </summary>
    public class OpenSearchClientOptions
    {
        /// <summary>
        /// Retrieves the current <see cref="JsonSerializerOptions"/> used by the <see cref="OpenSearchClient"/>
        /// </summary>
        public JsonSerializerOptions JsonSerializerOptions { get; } =
            new JsonSerializerOptions() { UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip };

        /// <summary>
        /// Retrieves the current <see cref="ConnectionSettings"/> used by the <see cref="OpenSearchClient"/>
        /// </summary>
        internal Action<ConnectionSettings>? InternalConnectionSettings { get; set; }

        /// <summary>
        /// Configures the <see cref="JsonSerializerOptions"/> used by the <see cref="OpenSearchClient"/>
        /// </summary>
        /// <param name="configure">Action which configures used <see cref="JsonSerializerOptions"/></param>
        /// <returns>This <see cref="OpenSearchClientOptions"/> instance for method chaining.</returns>
        public OpenSearchClientOptions WithJsonSerializerOptions(
            Action<JsonSerializerOptions> configure
        )
        {
            configure(JsonSerializerOptions);
            return this;
        }

        /// <summary>
        /// Configures an internal <see cref="ConnectionSettings"/> used by the <see cref="OpenSearchClient"/>
        /// </summary>
        /// <param name="configure">Action which configures used <see cref="ConnectionSettings"/></param>
        /// <returns>This <see cref="OpenSearchClientOptions"/> instance for method chaining.</returns>
        public OpenSearchClientOptions WithConnectionSettings(
            Action<ConnectionSettings> configure
        )
        {
            InternalConnectionSettings = configure;
            return this;
        }
    }
}
