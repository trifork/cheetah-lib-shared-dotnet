using System.Net.Http;
using Cheetah.Auth.Authentication;
using Microsoft.Extensions.DependencyInjection;
using OpenSearch.Net;

namespace Cheetah.OpenSearch.Connection
{
    internal sealed class CheetahOpenSearchConnection : HttpConnection
    {
        private readonly ITokenService _tokenService;

        public CheetahOpenSearchConnection([FromKeyedServices("opensearch")] ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        protected override HttpMessageHandler CreateHttpClientHandler(RequestData requestData)
        {
            return new OAuth2HttpMessageHandler(_tokenService, base.CreateHttpClientHandler(requestData));
        }
    }
}
