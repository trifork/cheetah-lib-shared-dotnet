using System.Text;
using Elasticsearch.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;
using Cheetah.WebApi.Shared.Infrastructure.Services.IndexAccess;
using Cheetah.template.WebApi.Core.Config; // TODO rename me from .template. to .Shared.
using Cheetah.template.WebApi.Core.Interfaces;
using Cheetah.WebApi.Shared.Middleware.Metric;
using Cheetah.template.WebApi.Util;

namespace Cheetah.Shared.WebApi.Infrastructure.Services.ElasticSearch
{
    public class CheetahElasticClient : IElasticSearch
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
            // Move this config to a lib class and to a config file? 
            // We should need to have some defaults when initializing the client
            _elasticClient = new ElasticClient(settings);
        }

        /// <summary>
        /// queries the ElasticSearch instance defined in appsettings.json, for all indecies-names using Nest.
        /// </summary>
        /// <returns>A List containing all index-names</returns>
        public async Task<List<string>> GetIndicies(List<IndexDescriptor> indices)
        {

            var result = await _elasticClient.Indices.GetAsync(new GetIndexRequest(Indices.All));
            var resultList = result.Indices.Select(index => index.Key.ToString()).ToList();
            resultList.RemoveAll(x => x.StartsWith('.'));
            return resultList;
        }
    }
}
