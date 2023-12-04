using Cheetah.Auth.Configuration;
using Cheetah.OpenSearch.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting.Internal;
using OpenSearch.Client;

namespace Cheetah.ComponentTest.OpenSearch
{
    /// <summary>
    /// Factory used for creating instances of <see cref="OpenSearchClient"/> which have pre-configured OAuth2 authentication.
    /// </summary>
    public static class OpenSearchClientFactory
    {
        private const string OPENSEARCH_PREFIX = "OPENSEARCH:";
        private const string URL_KEY = OPENSEARCH_PREFIX + "URL";
        private const string CLIENT_ID_KEY = OPENSEARCH_PREFIX + "CLIENTID";
        private const string CLIENT_SECRET_KEY = OPENSEARCH_PREFIX + "CLIENTSECRET";
        private const string OAUTHSCOPE_KEY = OPENSEARCH_PREFIX + "OAUTHSCOPE";
        private const string AUTHENDPOINT_KEY = OPENSEARCH_PREFIX + "AUTHENDPOINT";

        /// <summary>
        /// Creates a new instance of an <see cref="OpenSearchClient"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Retrieves necessary configuration from the provided <see cref="IConfiguration"/>
        /// </para>
        /// <para>
        /// Requires the following keys to be set in the provided configuration:
        /// <list type="table">
        ///     <listheader>
        ///        <term>Key</term>
        ///        <description>Description</description>
        ///     </listheader>
        ///     <item>
        ///         <term><c>OPENSEARCH:URL</c></term>
        ///         <description>Required - The URL where OpenSearch can be reached</description>
        ///     </item>
        ///     <item>
        ///         <term><c>OPENSEARCH:CLIENTID</c></term>
        ///         <description>Required - The clientId to use when authenticating towards OpenSearch</description>
        ///     </item>
        ///     <item>
        ///         <term><c>OPENSEARCH:CLIENTSECRET</c></term>
        ///         <description>Required - The client secret to use when authenticating towards OpenSearch</description>
        ///     </item>
        ///     <item>
        ///         <term><c>OPENSEARCH:AUTHENDPOINT</c></term>
        ///         <description>Required - The endpoint to retrieve authentication tokens from</description>
        ///     </item>
        ///     <item>
        ///         <term><c>OPENSEARCH:OAUTHSCOPE</c></term>
        ///         <description>Optional - The scope to request when retrieving authentication tokens</description>
        ///     </item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <param name="configuration">The configuration to use when constructing the client</param>
        /// <returns>The created <see cref="OpenSearchClient"/></returns>
        public static IOpenSearchClient Create(IConfiguration configuration)
        {
            var url = configuration.GetValue<string>(URL_KEY);
            var clientId = configuration.GetValue<string>(CLIENT_ID_KEY);
            var clientSecret = configuration.GetValue<string>(CLIENT_SECRET_KEY);
            var oauthScope = configuration.GetValue<string?>(OAUTHSCOPE_KEY);
            var authEndpoint = configuration.GetValue<string>(AUTHENDPOINT_KEY);

            var config = new OpenSearchConfig
            {
                AuthMode = OpenSearchConfig.OpenSearchAuthMode.OAuth2,
                Url = url,
                OAuth2 = new OAuth2Config()
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    AuthScope = oauthScope,
                    TokenEndpoint = authEndpoint
                }
            };

            var env = new HostingEnvironment { EnvironmentName = "Development" };

            return Cheetah.OpenSearch.OpenSearchClientFactory.CreateClientFromConfiguration(config, env);
        }
    }
}
