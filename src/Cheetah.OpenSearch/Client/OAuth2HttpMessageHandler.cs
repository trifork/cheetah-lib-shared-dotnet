using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cheetah.Core.Authentication;

namespace Cheetah.OpenSearch.Client
{
    internal sealed class OAuth2HttpMessageHandler : DelegatingHandler
    {
        private readonly ITokenService tokenService;

        public OAuth2HttpMessageHandler(ITokenService tokenService, HttpMessageHandler innerHandler) : base(innerHandler)
        {
            this.tokenService = tokenService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            var cachedAccessToken = await tokenService.RequestAccessTokenCachedAsync(cancellationToken);
            if (cachedAccessToken == null || string.IsNullOrEmpty(cachedAccessToken.AccessToken))
            {
                throw new UnauthorizedAccessException(
                    "Could not retrieve access token from IDP. Look at environment values to ensure they are correct"
                );
            }
            request.Headers.Add("Authorization", $"bearer {cachedAccessToken.AccessToken}");
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
