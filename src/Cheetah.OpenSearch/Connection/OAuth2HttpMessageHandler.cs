using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cheetah.Auth.Authentication;

namespace Cheetah.OpenSearch.Connection
{
    internal sealed class OAuth2HttpMessageHandler : DelegatingHandler
    {
        private readonly ITokenService _tokenService;

        public OAuth2HttpMessageHandler(ITokenService tokenService, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            this._tokenService = tokenService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            var accessToken = _tokenService.RequestAccessToken();
            if (string.IsNullOrEmpty(accessToken.AccessToken))
            {
                throw new UnauthorizedAccessException(
                    "Could not retrieve access token from IDP. Look at environment values to ensure they are correct"
                );
            }
            request.Headers.Add("Authorization", $"bearer {accessToken.AccessToken}");
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
