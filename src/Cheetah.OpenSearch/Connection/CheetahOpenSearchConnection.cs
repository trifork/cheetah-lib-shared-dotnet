using System.Net.Http;
using Cheetah.Auth.Authentication;
using OpenSearch.Net;

namespace Cheetah.OpenSearch.Connection
{
    internal sealed class CheetahOpenSearchConnection : HttpConnection
    {
        private readonly ITokenService _tokenService;

        public CheetahOpenSearchConnection(ITokenService tokenService)
        {
            this._tokenService = tokenService;
        }

        protected override HttpMessageHandler CreateHttpClientHandler(RequestData requestData)
            => new OAuth2HttpMessageHandler(_tokenService, base.CreateHttpClientHandler(requestData));
    }
}
