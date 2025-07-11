using System;
using System.Text;
using Cheetah.OpenSearch.Configuration;
using Cheetah.OpenSearch.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly OpenSearchClientOptions _clientOptions;
        private readonly IConnection? _connection;

        /// <summary>
        /// Create a new instance of <see cref="OpenSearchClientFactory"/>
        /// </summary>
        /// <param name="clientConfig">The <see cref="OpenSearchConfig"/> to use for configuring the client.</param>
        /// <param name="clientLogger">The <see cref="ILogger{OpenSearchClient}"/> to use for logging in the client.</param>
        /// <param name="logger">The <see cref="ILogger{OpenSearchClientFactory}"/> to use for logging in the factory.</param>
        /// <param name="clientOptions">The <see cref="OpenSearchClientOptions"/> used to modify client behavior</param>
        /// <param name="connectionPool">The <see cref="IConnectionPool"/> to use for generated clients.</param>
        /// <param name="connection">The <see cref="IConnection"/> to use for generated clients.</param>
        public OpenSearchClientFactory(
            IOptions<OpenSearchConfig> clientConfig,
            ILogger<OpenSearchClient> clientLogger,
            ILogger<OpenSearchClientFactory> logger,
            OpenSearchClientOptions clientOptions,
            IConnectionPool connectionPool,
            IConnection? connection = null
        )
        {
            clientConfig.Value.Validate();
            _clientConfig = clientConfig.Value;
            _clientLogger = clientLogger;
            _logger = logger;
            _clientOptions = clientOptions;
            _connectionPool = connectionPool;
            _connection = connection;
        }

        /// <summary>
        /// Create a new, pre-configured <see cref="OpenSearchClient"/> instance
        /// </summary>
        /// <returns></returns>
        public OpenSearchClient CreateOpenSearchClient()
        {
            _logger.LogInformation(
                "Creating OpenSearchClient. Authentication is {authMode}",
                GetAuthModeLogString()
            );
            var connectionSettings = GetDefaultConnectionSettings();
            _clientOptions.InternalConnectionSettings?.Invoke(connectionSettings);
            return new OpenSearchClient(connectionSettings);
        }

        private string GetAuthModeLogString()
        {
            return _clientConfig.AuthMode switch
            {
                OpenSearchConfig.OpenSearchAuthMode.None => "disabled",
                OpenSearchConfig.OpenSearchAuthMode.Basic
                    => $"enabled using Basic Auth, username=${_clientConfig.UserName}",
                OpenSearchConfig.OpenSearchAuthMode.OAuth2
                    => $"enabled using OAuth2, clientId=${_clientConfig.OAuth2.ClientId}",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private ConnectionSettings GetDefaultConnectionSettings()
        {
            return new ConnectionSettings(
                _connectionPool,
                _connection, // If this is null, a default connection will be used.
                GetSourceSerializerFactory()
            )
                .ConfigureBasicAuthIfEnabled(_clientConfig)
                .ConfigureTlsValidation(_clientConfig)
                .OnRequestCompleted(LogRequestBody)
                .ThrowExceptions();
        }

        private ConnectionSettings.SourceSerializerFactory GetSourceSerializerFactory()
        {
            return (builtin, settings) =>
                new JsonNetSerializer(
                    builtin,
                    settings,
                    () => _clientOptions.JsonSerializerSettings
                );
        }

        private void LogRequestBody(IApiCallDetails apiCallDetails)
        {
            // Only call this if the relevant log level is enabled, in order to avoid unnecessary allocations and decoding
            if (
                apiCallDetails.RequestBodyInBytes != null
                && _clientLogger.IsEnabled(LogLevel.Debug)
            )
            {
                _clientLogger.LogDebug(
                    "Sent raw query: {json}",
                    Encoding.UTF8.GetString(apiCallDetails.RequestBodyInBytes)
                );
            }
        }
    }
}
