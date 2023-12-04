using System;
using System.Text;
using Cheetah.Auth.Authentication;
using Cheetah.Auth.Util;
using Cheetah.OpenSearch.Configuration;
using Cheetah.OpenSearch.Connection;
using Cheetah.OpenSearch.Extensions;
using Cheetah.OpenSearch.Util;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenSearch.Client;
using OpenSearch.Client.JsonNetSerializer;
using OpenSearch.Net;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Cheetah.OpenSearch
{
    /// <summary>
    /// Factory for creating <see cref="OpenSearchClient"/> instances
    /// </summary>
    public class OpenSearchClientFactory
    {
        private readonly ILogger<OpenSearchClientFactory> _logger;
        readonly IConnectionPool _connectionPool;
        private readonly ILogger<OpenSearchClient> _clientLogger;
        private readonly OpenSearchConfig _clientConfig;
        private readonly IHostEnvironment? _hostEnvironment;
        private readonly IConnection? _connection;

        /// <summary>
        /// Create a new instance of <see cref="OpenSearchClientFactory"/>
        /// </summary>
        /// <param name="clientConfig">The <see cref="OpenSearchConfig"/> to use for configuring the client.</param>
        /// <param name="clientLogger">The <see cref="ILogger{OpenSearchClient}"/> to use for logging in the client.</param>
        /// <param name="logger">The <see cref="ILogger{OpenSearchClientFactory}"/> to use for logging in the factory.</param>
        /// <param name="connectionPool">The <see cref="IConnectionPool"/> to use for generated clients.</param>
        /// <param name="connection">The <see cref="IConnection"/> to use for generated clients.</param>
        /// <param name="hostEnvironment">The <see cref="IHostEnvironment"/> that the client is running in.</param>
        public OpenSearchClientFactory(
            IOptions<OpenSearchConfig> clientConfig,
            ILogger<OpenSearchClient> clientLogger,
            ILogger<OpenSearchClientFactory> logger,
            IConnectionPool connectionPool,
            IConnection? connection = null,
            IHostEnvironment? hostEnvironment = null
        )
        {
            clientConfig.Value.ValidateConfig();
            _clientConfig = clientConfig.Value;
            _hostEnvironment = hostEnvironment;
            _clientLogger = clientLogger;
            _logger = logger;
            _connectionPool = connectionPool;
            _connection = connection;
        }

        /// <summary>
        /// Create a new, pre-configured <see cref="OpenSearchClient"/> instance
        /// </summary>
        /// <returns></returns>
        public OpenSearchClient CreateOpenSearchClient()
        {
            _logger.LogInformation("Creating OpenSearchClient. Authentication is {authMode}", GetAuthModeLogString());
            return new OpenSearchClient(GetConnectionSettings());
        }

        string GetAuthModeLogString() => 
            _clientConfig.AuthMode switch {
                OpenSearchConfig.OpenSearchAuthMode.None => "disabled",
                OpenSearchConfig.OpenSearchAuthMode.Basic => $"enabled using Basic Auth, username=${_clientConfig.UserName}",
                OpenSearchConfig.OpenSearchAuthMode.OAuth2 => $"enabled using OAuth2, clientId=${_clientConfig.OAuth2.ClientId}",
                _ => throw new ArgumentOutOfRangeException()
            };

        private ConnectionSettings GetConnectionSettings()
        {
            // TODO: We should need to have some defaults when initializing the client
            // TODO: dive down in the settings for OpenSearch and see if we need to expose any of the options as easily changeable
            return new ConnectionSettings(
                    _connectionPool,
                    _connection, // If this is null, a default connection will be used.
                    GetSourceSerializerFactory()
                )
                .ConfigureBasicAuthIfEnabled(_clientConfig)
                .ConfigureTlsValidation(_clientConfig)
                .OnRequestCompleted(LogRequestBody)
                .ThrowExceptions()
                .DisableDirectStreaming(ShouldDisableDirectStreaming());
        }


        private static ConnectionSettings.SourceSerializerFactory GetSourceSerializerFactory()
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            
            jsonSerializerSettings.Converters.Add(new UtcDateTimeConverter());
            
            return (builtin, settings) => new JsonNetSerializer(
                builtin,
                settings,
                () => jsonSerializerSettings 
            );
        }

        private bool ShouldDisableDirectStreaming()
        {
            bool shouldDisableDirectStreaming = _hostEnvironment?.IsDevelopment() ?? false; // Assume production mode if we can't determine the environment
            if (shouldDisableDirectStreaming)
            {
                _logger.LogWarning("OpenSearch direct streaming is disabled, which allows easier debugging, but potentially impacts performance. This should only be enabled in development mode.");   
            }
            return shouldDisableDirectStreaming;
        }
        
        private void LogRequestBody(IApiCallDetails apiCallDetails)
        {
            // Only call this if the relevant log level is enabled, in order to avoid unnecessary allocations and decoding
            if (apiCallDetails.RequestBodyInBytes != null && _clientLogger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Sent raw query: {json}", Encoding.UTF8.GetString(apiCallDetails.RequestBodyInBytes));
            }
        }
        
        
        /// <summary>
        /// Creates an IOpenSearchClient from the provided configuration.
        /// </summary>
        /// <remarks>
        /// <b>WARNING</b>: This method should <i>only</i> be used if you for some reason cannot use dependency injection and need to create a client manually.
        /// In any other circumstances, you should use the <see cref="ServiceCollectionExtensions.AddCheetahOpenSearch"/> method during service registration and inject <see cref="IOpenSearchClient"/> into your service.
        /// </remarks>
        /// <param name="config">The configuration to create the client from</param>
        /// <param name="hostEnvironment">An optional host environment, used to determine whether additional debug information should be available on the returned client</param>
        /// <returns>A pre-configured <see cref="OpenSearchClient"/></returns>
        public static IOpenSearchClient CreateClientFromConfiguration(OpenSearchConfig config, IHostEnvironment? hostEnvironment = null)
        {
            config.ValidateConfig();

            var loggerFactory = new LoggerFactory();
            var options = Options.Create<OpenSearchConfig>(config);
            IConnection? connection = null;
            if (config.AuthMode == OpenSearchConfig.OpenSearchAuthMode.OAuth2)
            {
                var tokenService = new OAuth2TokenService(
                    loggerFactory.CreateLogger<OAuth2TokenService>(), 
                    new DefaultHttpClientFactory(),
                    new MemoryCache(new MemoryCacheOptions()), 
                    Options.Create(config.OAuth2),
                    "opensearch-access-token");
                connection = new CheetahOpenSearchConnection(tokenService);
            }
            
            return new OpenSearchClientFactory(
                    options, 
                    new Logger<OpenSearchClient>(loggerFactory),
                    new Logger<OpenSearchClientFactory>(loggerFactory),
                    ConnectionPoolHelper.GetConnectionPool(config.Url),
                    connection: connection,
                    hostEnvironment: hostEnvironment)
                .CreateOpenSearchClient();
        }
    }
}
