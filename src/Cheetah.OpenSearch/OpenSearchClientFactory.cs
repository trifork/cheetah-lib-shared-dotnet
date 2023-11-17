using System;
using System.Linq;
using System.Text;
using Cheetah.Core.Authentication;
using Cheetah.Core.Util;
using Cheetah.OpenSearch.Client;
using Cheetah.OpenSearch.Config;
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
    public class OpenSearchClientFactory
    {
        private readonly ITokenService _tokenService;
        private readonly ILogger<OpenSearchClientFactory> _logger;
        private readonly ILogger<OpenSearchClient> _clientLogger;
        private readonly OpenSearchConfig _clientConfig;
        private readonly IHostEnvironment _hostEnvironment;

        public OpenSearchClientFactory(
            ITokenService tokenService, 
            IOptions<OpenSearchConfig> clientConfig,
            IHostEnvironment hostEnvironment,
            ILogger<OpenSearchClient> clientLogger,
            ILogger<OpenSearchClientFactory> logger)
        {
            _tokenService = tokenService;
            _clientConfig = clientConfig.Value;
            _clientConfig.ValidateConfig();
            _hostEnvironment = hostEnvironment;
            _clientLogger = clientLogger;
            _logger = logger;
        }

        public OpenSearchClient CreateOpenSearchClient()
        {
            _logger.LogInformation("Creating OpenSearchClient. Authentication is {authMode}", GetAuthModeLogString());
            return new OpenSearchClient(GetConnectionSettings());
        }

        string GetAuthModeLogString() => 
            _clientConfig.AuthMode switch {
                OpenSearchConfig.OpenSearchAuthMode.None => "disabled",
                OpenSearchConfig.OpenSearchAuthMode.Basic => $"enabled using Basic Auth, username=${_clientConfig.UserName}",
                OpenSearchConfig.OpenSearchAuthMode.OAuth2 => $"enabled using OAuth2, clientId=${_clientConfig.ClientId}",
                _ => throw new ArgumentOutOfRangeException()
            };

        private ConnectionSettings GetConnectionSettings()
        {
            // TODO: We should need to have some defaults when initializing the client
            // TODO: dive down in the settings for OpenSearch and see if we need to expose any of the options as easily changeable
            return new ConnectionSettings(
                    GetConnectionPool(),
                    GetConnection(),
                    GetSourceSerializerFactory()
                )
                .ConfigureBasicAuthIfEnabled(_clientConfig)
                .ConfigureTlsValidation(_clientConfig)
                .OnRequestCompleted(LogRequestBody)
                .ThrowExceptions()
                .DisableDirectStreaming(_hostEnvironment.IsDevelopment());
        }

        private CheetahOpenSearchConnection? GetConnection()
        {
            return _clientConfig.AuthMode switch
            {
                OpenSearchConfig.OpenSearchAuthMode.None => null, // providing a null connection will use the default in OpenSearchClient
                OpenSearchConfig.OpenSearchAuthMode.Basic => null,
                OpenSearchConfig.OpenSearchAuthMode.OAuth2 => new CheetahOpenSearchConnection(_tokenService),
                _ => throw new ArgumentOutOfRangeException()
            };
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
        
        private IConnectionPool GetConnectionPool()
        {
            string urlString = _clientConfig.Url;
            if (urlString.Contains(','))
            {
                return new StaticConnectionPool(urlString.Split(',').Select(url => new Uri(url)));
            }
            
            return new SingleNodeConnectionPool(new Uri(urlString));
        }
        
        private void LogRequestBody(IApiCallDetails apiCallDetails)
        {
            // Only call this if the relevant log level is enabled, in order to avoid unnecessary allocations and decoding
            if (apiCallDetails.RequestBodyInBytes != null && _clientLogger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Sent raw query: {json}", Encoding.UTF8.GetString(apiCallDetails.RequestBodyInBytes));
            }
        }
    }
}
