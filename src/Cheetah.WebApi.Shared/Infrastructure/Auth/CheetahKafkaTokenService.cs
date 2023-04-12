using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Cheetah.WebApi.Shared.Infrastructure.Auth
{
    public class CheetahKafkaTokenService : TokenService
    {
        public CheetahKafkaTokenService(ILogger logger, IHttpClientFactory httpClientFactory, IMemoryCache cache, string clientId, string clientSecret, string tokenEndpoint)
        : base(logger, httpClientFactory, cache, clientId, clientSecret, tokenEndpoint)
        {
        }


        public override string CacheKey => "kafka-access-token";
    }
}