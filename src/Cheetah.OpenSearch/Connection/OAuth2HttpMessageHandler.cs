using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cheetah.Core.Authentication;

namespace Cheetah.OpenSearch.Connection
{
    internal sealed class OAuth2HttpMessageHandler : DelegatingHandler
    {
        private readonly ITokenService _tokenService;

        public OAuth2HttpMessageHandler(ITokenService tokenService, HttpMessageHandler innerHandler) : base(innerHandler)
        {
            this._tokenService = tokenService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            var accessToken = await _tokenService.RequestAccessTokenAsync(cancellationToken);
            if (accessToken == null || string.IsNullOrEmpty(accessToken.Value.AccessToken))
            {
                throw new UnauthorizedAccessException(
                    "Could not retrieve access token from IDP. Look at environment values to ensure they are correct"
                );
            }
            request.Headers.Add("Authorization", $"bearer {accessToken.Value.AccessToken}");
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
