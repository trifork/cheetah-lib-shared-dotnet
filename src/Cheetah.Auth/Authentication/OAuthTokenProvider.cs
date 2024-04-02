using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cheetah.Auth.Configuration;
using IdentityModel.Client;
using Microsoft.Extensions.Options;

namespace Cheetah.Auth.Authentication
{
    public class OAuthTokenProvider : ICachableTokenProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        
        private readonly string _cacheKey;
        private readonly OAuth2Config _config;

        public OAuthTokenProvider(IOptions<OAuth2Config> config, IHttpClientFactory httpClientFactory, string cacheKey)
        {
            _cacheKey = cacheKey;
            _httpClientFactory = httpClientFactory;
            _config = config.Value;
        }
        

        public async Task<TokenResponse?> GetTokenResponse(CancellationToken cancellationToken)
        {
            using var httpClient = _httpClientFactory.CreateClient(_cacheKey);
            var tokenClient = new TokenClient(
                httpClient,
                new TokenClientOptions
                {
                    Address = _config.TokenEndpoint,
                    ClientId = _config.ClientId,
                    ClientSecret = _config.ClientSecret
                }
            );
            var tokenResponse = await tokenClient
                .RequestClientCredentialsTokenAsync(
                    scope: _config.Scope,
                    cancellationToken: cancellationToken
                )
                .ConfigureAwait(false);

            return tokenResponse;
        }
    }
}
