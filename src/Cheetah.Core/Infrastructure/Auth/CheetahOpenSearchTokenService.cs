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
            string tokenEndpoint
        )
            : base(logger, httpClientFactory, cache, clientId, clientSecret, tokenEndpoint) { }

        public override string CacheKey => "opensearch-access-token";
    }
}
