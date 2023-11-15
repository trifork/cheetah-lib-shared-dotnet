using System.Net.Http;
using Cheetah.Core.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OpenSearch.Net;

namespace Cheetah.OpenSearch.Client
{
    internal sealed class CheetahOpenSearchConnection : HttpConnection
    {
        private readonly TokenService tokenService;

        public CheetahOpenSearchConnection(
            ILogger logger,
            IMemoryCache cache,
            IHttpClientFactory httpClientFactory,
            string clientId,
            string clientSecret,
            string tokenEndpoint,
            string? oauthScope = null
        )
        {
            tokenService = new CheetahOpenSearchTokenService(
                logger,
                httpClientFactory,
                cache,
                clientId,
                clientSecret,
                tokenEndpoint,
                oauthScope
            );
        }

        private HttpMessageHandler InnerCreateHttpClientHandler(RequestData requestData)
        {
            return base.CreateHttpClientHandler(requestData);
        }

        protected override HttpMessageHandler CreateHttpClientHandler(RequestData requestData)
        {
            return new OAuth2HttpClientHandler(
                tokenService,
                InnerCreateHttpClientHandler(requestData)
            );
        }
    }
}
