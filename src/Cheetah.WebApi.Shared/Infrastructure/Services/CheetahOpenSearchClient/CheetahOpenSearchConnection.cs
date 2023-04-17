using System.Net.Http;
using Cheetah.WebApi.Shared.Infrastructure.Auth;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OpenSearch.Net;

namespace Cheetah.WebApi.Shared.Infrastructure.Services.CheetahOpenSearchClient
{
    internal class CheetahOpenSearchConnection : HttpConnection
    {
        private readonly TokenService tokenService;
        public CheetahOpenSearchConnection(ILogger logger, IMemoryCache cache, IHttpClientFactory httpClientFactory, string clientId, string clientSecret, string tokenEndpoint)
        {
            this.tokenService = new CheetahOpenSearchTokenService(logger, httpClientFactory, cache, clientId, clientSecret, tokenEndpoint);
        }

        protected virtual HttpMessageHandler InnerCreateHttpClientHandler(RequestData requestData) =>
                base.CreateHttpClientHandler(requestData);

        protected override HttpMessageHandler CreateHttpClientHandler(RequestData requestData) =>
            new OAuth2HttpClientHandler(this.tokenService, InnerCreateHttpClientHandler(requestData));

    }
}