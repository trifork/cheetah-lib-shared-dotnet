using System.Net.Http;
using Cheetah.Core.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Cheetah.OpenSearch
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
            string? oauthScope = null
        )
            : base(logger, httpClientFactory, cache, clientId, clientSecret, tokenEndpoint, oauthScope) { }

        public override string CacheKey => "opensearch-access-token";
    }
}
