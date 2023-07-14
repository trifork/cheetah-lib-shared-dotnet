using Cheetah.Core.Config;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Core.Infrastructure.Auth
{
    public class CheetahKafkaTokenService : TokenService
    {
        public CheetahKafkaTokenService(
            ILogger logger,
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            IOptions<KafkaConfig> kafkaConfig
        )
            : base(logger, httpClientFactory, cache, kafkaConfig.Value.ClientId, kafkaConfig.Value.ClientSecret, kafkaConfig.Value.TokenEndpoint, kafkaConfig.Value.OAuthScope) { }

        public override string CacheKey => "kafka-access-token";
    }
}
