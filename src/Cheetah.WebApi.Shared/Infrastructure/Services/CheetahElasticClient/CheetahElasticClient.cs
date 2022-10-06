using System.Text;
using Elasticsearch.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;
using Cheetah.WebApi.Shared.Infrastructure.Services.IndexAccess;
using Cheetah.Shared.WebApi.Core.Config;
using Cheetah.Shared.WebApi.Core.Interfaces;
using Cheetah.WebApi.Shared.Middleware.Metric;
using Cheetah.Shared.WebApi.Util;

namespace Cheetah.Shared.WebApi.Infrastructure.Services.CheetahElasticClient
{
    /// <summary>Wrapper around ElasticClient, which introduces logging, authorization, and metrics
    /// <para>
    /// CheetahElasticClient is a controller interface for accessing ElasticSearch's API.
    /// It provides:
    /// <list type=">">
    /// <item>
    /// <term>Access control</term>
    /// <description>
    /// Callers of this class would be able to access only indices on ES that they are authorized to access
    /// </description>
    /// </item>
    /// <item>
    /// <term>Logging</term>
    /// <description>
    /// When debug mode is enabled the raw requests to ES are logged in plaintext
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
    public class CheetahElasticClient : ICheetahElasticClient
    {
        private readonly ILogger<CheetahElasticClient> _logger;
        private readonly ElasticClient _elasticClient;
        private readonly ElasticConfig _elasticConfig;
        private readonly IMetricReporter _metricReporter;

        public CheetahElasticClient(IOptions<ElasticConfig> config, IHostEnvironment hostEnvironment,
            ILogger<CheetahElasticClient> logger, IMetricReporter metricReporter)
        {
            _logger = logger;
            _elasticConfig = config.Value;
            _metricReporter = metricReporter;
            var pool = new SingleNodeConnectionPool(new Uri(_elasticConfig.Url));
            var settings = new ConnectionSettings(pool,
                (builtin, settings) =>
                {
                    var jsonSerializerSettings = new JsonSerializerSettings()
                    {
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };
                    jsonSerializerSettings.Converters.Add(new EpochDateTimeConverter());
                    return new JsonNetSerializer(builtin, settings, () => jsonSerializerSettings);
                })
                .BasicAuthentication(_elasticConfig.UserName, _elasticConfig.Password)
                .ThrowExceptions();

            settings.OnRequestCompleted(apiCallDetails =>
            {
                if (apiCallDetails.RequestBodyInBytes != null)
                {
                    var json = Encoding.UTF8.GetString(apiCallDetails.RequestBodyInBytes);
                    _logger.LogDebug("Sent raw query: {@json}", json);
                }
            });
            if (hostEnvironment.IsDevelopment()) settings.DisableDirectStreaming(true); //Enables data in OnRequestCompleted callback

            // TODO: We should need to have some defaults when initializing the client
            // TODO: dive down in the settings for elastic and see if we need to expose any of the options as easily changeable
            _elasticClient = new ElasticClient(settings);
        }

        /// <summary>
        /// Queries the ElasticSearch instance defined in appsettings.json, for all indices' names
        /// </summary>
        /// <returns>A List containing all index-names</returns>
        public async Task<List<string>> GetIndices(List<IndexDescriptor> indices)
        {
            var result = await _elasticClient.Indices.GetAsync(new GetIndexRequest(Indices.All));
            return result.Indices.Select(index => index.Key.ToString())
                                 .Where(x => !x.StartsWith('.'))
                                 .ToList();
        }
    }
}
