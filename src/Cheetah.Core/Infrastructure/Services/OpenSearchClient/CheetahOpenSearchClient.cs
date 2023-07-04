using System.Text;
using Cheetah.Core.Config;
using Cheetah.Core.Infrastructure.Services.IndexAccess;
using Cheetah.Core.Interfaces;
using Cheetah.WebApi.Shared.Util;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenSearch.Client;
using OpenSearch.Client.JsonNetSerializer;
using OpenSearch.Net;

namespace Cheetah.Core.Infrastructure.Services.OpenSearchClient
{
    /// <summary>Wrapper around OpenSearch, which introduces logging, authorization, and metrics
    /// <para>
    /// CheetahOpenSearchClient is a controller interface for accessing OpenSearch's API.
    /// It provides:
    /// <list type="bullet">
    /// <item>
    /// <term>Access control</term>
    /// <description>
    /// Callers of this class would be able to access only indices on OpenSearch that they are authorized to access
    /// </description>
    /// </item>
    /// <item>
    /// <term>Logging</term>
    /// <description>
    /// When debug mode is enabled the raw requests to OpenSearch are logged in plaintext
    /// </description>
    /// </item>
    /// <item>
    /// <term>Metrics</term>
    /// <description>
    /// Automatic metrics for requests done through this client
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </summary>
    public class CheetahOpenSearchClient : ICheetahOpenSearchClient
    {
        private readonly ILogger<CheetahOpenSearchClient> _logger;
        public OpenSearch.Client.OpenSearchClient InternalClient { get; }
        private readonly OpenSearchConfig _openSearchConfig;

        private Func<JsonSerializerSettings>? jsonSerializerSettingsFactory;

        public CheetahOpenSearchClient(
            IMemoryCache cache,
            IHttpClientFactory httpClientfactory,
            IOptions<OpenSearchConfig> openSearchConfig,
            IHostEnvironment hostEnvironment,
            ILogger<CheetahOpenSearchClient> logger
        )
        {
            _logger = logger;
            _openSearchConfig = openSearchConfig.Value;
            IConnectionPool pool;
            if (_openSearchConfig.Url.Contains(','))
            {
                // SniffingConnectionPool
                // todo: ensure monitoring_user role
                // Unless you configure the publish host option, the sniffing result will be unusable.
                pool = new StaticConnectionPool(
                    _openSearchConfig.Url.Split(',').Select(url => new Uri(url))
                );
            }
            else
            {
                pool = new SingleNodeConnectionPool(new Uri(_openSearchConfig.Url));
            }
            IConnection? cheetahConnection = null;
            _openSearchConfig.ValidateConfig();

            if (_openSearchConfig.AuthMode == OpenSearchConfig.OpenSearchAuthMode.OAuth2)
            {
                logger.LogInformation(
                    "Enabled OAuth2 for OpenSearch with clientid={clientId}",
                    _openSearchConfig.ClientId
                );
                cheetahConnection = new CheetahOpenSearchConnection(
                    logger,
                    cache,
                    httpClientfactory,
                    _openSearchConfig.ClientId,
                    _openSearchConfig.ClientSecret,
                    _openSearchConfig.TokenEndpoint
                );
            }
            var settings = new ConnectionSettings(
                pool,
                cheetahConnection,
                (builtin, settings) =>
                {
                    return new JsonNetSerializer(
                        builtin,
                        settings,
                        GetJsonSerializerSettingsFactory()
                    );
                }
            ).ThrowExceptions();
            settings = settings.ServerCertificateValidationCallback(
                CertificateValidations.AllowAll
            );
            if (_openSearchConfig.AuthMode == OpenSearchConfig.OpenSearchAuthMode.BasicAuth)
            {
                logger.LogInformation(
                    "Enabled BasicAuth for OpenSearch with username={username}",
                    _openSearchConfig.UserName
                );
                settings = settings.BasicAuthentication(
                    _openSearchConfig.UserName,
                    _openSearchConfig.Password
                );
            }

            settings.OnRequestCompleted(apiCallDetails =>
            {
                if (apiCallDetails.RequestBodyInBytes != null)
                {
                    var json = Encoding.UTF8.GetString(apiCallDetails.RequestBodyInBytes);
                    _logger.LogDebug("Sent raw query: {@json}", json);
                }
            });
            if (hostEnvironment.IsDevelopment())
                settings.DisableDirectStreaming(true); //Enables data in OnRequestCompleted callback

            // TODO: We should need to have some defaults when initializing the client
            // TODO: dive down in the settings for OpenSearch and see if we need to expose any of the options as easily changeable
            InternalClient = new OpenSearch.Client.OpenSearchClient(settings);
        }

        #region GetJsonSerializerSettingsFactory
        private Func<JsonSerializerSettings> GetJsonSerializerSettingsFactory()
        {
            if (jsonSerializerSettingsFactory == null) // Get default
            {
                var jsonSerializerSettings = new JsonSerializerSettings()
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                jsonSerializerSettings.Converters.Add(new UtcDateTimeConverter());
                return () => jsonSerializerSettings;
            }
            return jsonSerializerSettingsFactory;
        }
        #endregion
        /// <summary>
        /// Set what JSON settings to use when deserializing data from OpenSearch
        /// </summary>
        /// <param name="jsonSerializerSettingsFactory"></param>
        public void SetJsonSerializerSettingsFactory(
            Func<JsonSerializerSettings> jsonSerializerSettingsFactory
        )
        {
            this.jsonSerializerSettingsFactory = jsonSerializerSettingsFactory;
        }

        /// <summary>
        /// Queries the OpenSearch instance for all indices' names
        /// </summary>
        /// <returns>A List containing all index-names</returns>
        public async Task<List<string>> GetIndices(List<IndexDescriptor> indices)
        {
            var result = await InternalClient.Indices.GetAsync(new GetIndexRequest(Indices.All));
            return result.Indices
                .Select(index => index.Key.ToString())
                .Where(x => !x.StartsWith('.'))
                .ToList();
        }
    }
}
