using System.Net.Http;
using Cheetah.Core.Authentication;
using Cheetah.OpenSearch.Config;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.OpenSearch
{
    public class CheetahOpenSearchTokenService : TokenService
    {
        public CheetahOpenSearchTokenService(
            ILogger<CheetahOpenSearchTokenService> logger,
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            IOptions<OpenSearchConfig> config
        )
            : base(logger, httpClientFactory, cache, config.Value.ClientId, config.Value.ClientSecret, config.Value.TokenEndpoint, config.Value.OAuthScope) { }

        public override string CacheKey => "opensearch-access-token";
    }
}
