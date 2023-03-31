using Cheetah.WebApi.Shared.Infrastructure.Auth;

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
            request.Headers.Add("Authorization", $"bearer {cachedAccessToken}");
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}