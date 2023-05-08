using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KafkaCore
{
    public class CheetahKafkaTokenService : TokenService
    {
        public CheetahKafkaTokenService(ILogger logger, IHttpClientFactory httpClientFactory, IMemoryCache cache, IOptions<KafkaConfig> config)
            : base(logger, httpClientFactory, cache, config.Value.ClientId, config.Value.ClientSecret, config.Value.TokenEndpoint)
        {
        }


        public override string CacheKey => "kafka-access-token";
    }
}
