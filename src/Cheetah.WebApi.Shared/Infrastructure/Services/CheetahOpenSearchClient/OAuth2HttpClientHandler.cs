using Cheetah.WebApi.Shared.Infrastructure.Auth;
using System;

namespace Cheetah.Shared.WebApi.Infrastructure.Services.CheetahOpenSearchClient
{
    internal class OAuth2HttpClientHandler : DelegatingHandler
    {

        private readonly TokenService tokenService;


        public OAuth2HttpClientHandler(TokenService tokenService, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            this.tokenService = tokenService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var cachedAccessToken = await this.tokenService.RequestAccessTokenCachedAsync(cancellationToken);
            if (cachedAccessToken == null || string.IsNullOrEmpty(cachedAccessToken.AccessToken))
            {
                throw new UnauthorizedAccessException("Could not retrieve access token from IDP. Look at environment values to ensure they are correct");
            }
            request.Headers.Add("Authorization", $"bearer {cachedAccessToken.AccessToken}");
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}