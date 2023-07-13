using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Cheetah.Core.Infrastructure.Auth
{
    public class CheetahOpenSearchTokenService : TokenService
    {
        public CheetahOpenSearchTokenService(
            ILogger logger,
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            string clientId,
            string clientSecret,
            string tokenEndpoint,
            string? scope = null
        )
            : base(logger, httpClientFactory, cache, clientId, clientSecret, tokenEndpoint, scope) { }

        public override string CacheKey => "opensearch-access-token";
    }
}
