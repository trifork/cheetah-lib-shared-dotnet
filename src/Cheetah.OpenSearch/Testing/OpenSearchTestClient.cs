using Cheetah.Auth.Authentication;
using Cheetah.Auth.Util;
using Cheetah.OpenSearch.Configuration;
using Cheetah.OpenSearch.Connection;
using Cheetah.OpenSearch.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using OpenSearch.Net;

namespace Cheetah.OpenSearch.Testing
{
    /// <summary>
    /// Utility class for creating OpenSearch test clients
    /// </summary>
    public static class OpenSearchTestClient
    {
        /// <inheritdoc cref="Create(OpenSearchConfig,OpenSearchClientOptions?)"/>
        /// <param name="configuration">The <see cref="IConfiguration"/> to create the client from</param>
        /// <param name="options">The <see cref="OpenSearchClientOptions"/> used to modify client behavior</param>
        public static IOpenSearchClient Create(
            IConfiguration configuration,
            OpenSearchClientOptions? options = null
        )
        {
            var config = new OpenSearchConfig();
            configuration.Bind(OpenSearchConfig.Position, config);
            return Create(config, options);
        }

        /// <summary>
        /// Creates an IOpenSearchClient from the provided configuration.
        /// </summary>
        /// <remarks>
        /// <b>WARNING</b>: This method should <i>only</i> be used if you for some reason cannot use dependency injection and need to create a client manually.
        /// In any other circumstances, you should use the <see cref="ServiceCollectionExtensions.AddCheetahOpenSearch"/> method during service registration and inject <see cref="IOpenSearchClient"/> into your service.
        /// </remarks>
        /// <param name="configuration">The <see cref="OpenSearchConfig"/> to create the client from</param>
        /// <param name="options">The <see cref="OpenSearchClientOptions"/> used to modify client behavior</param>
        /// <returns>A pre-configured <see cref="OpenSearchClient"/></returns>
        public static IOpenSearchClient Create(
            OpenSearchConfig configuration,
            OpenSearchClientOptions? options = null
        )
        {
            configuration.Validate();

            var loggerFactory = new LoggerFactory();
            options ??= new OpenSearchClientOptions();

            IConnection? connection = null;
            if (configuration.AuthMode == OpenSearchConfig.OpenSearchAuthMode.OAuth2)
            {
                var tokenService = new OAuth2TokenService(
                    loggerFactory.CreateLogger<OAuth2TokenService>(),
                    new DefaultHttpClientFactory(),
                    new MemoryCache(new MemoryCacheOptions()),
                    Options.Create(configuration.OAuth2),
                    "opensearch-access-token"
                );
                connection = new CheetahOpenSearchConnection(tokenService);
            }

            return new OpenSearchClientFactory(
                Options.Create(configuration),
                new Logger<OpenSearchClient>(loggerFactory),
                new Logger<OpenSearchClientFactory>(loggerFactory),
                options,
                ConnectionPoolHelper.GetConnectionPool(configuration.Url),
                connection: connection
            ).CreateOpenSearchClient();
        }
    }
}
