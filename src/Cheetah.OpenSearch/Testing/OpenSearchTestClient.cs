using System.Threading.Tasks;
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
        /// <param name="configuration">The configuration to create the client from</param>
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
            var optionsOAuth2 = Options.Create(configuration.OAuth2);

            IConnection? connection = null;
            if (configuration.AuthMode == OpenSearchConfig.OpenSearchAuthMode.OAuth2)
            {
                var tokenService = new CachedTokenProvider(configuration.OAuth2,
                    new OAuthTokenProvider(configuration.OAuth2, new DefaultHttpClientFactory()),
                    loggerFactory.CreateLogger<CachedTokenProvider>());
                Task.Run(() => tokenService.StartAsync());
                
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
