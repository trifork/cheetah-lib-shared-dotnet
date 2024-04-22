using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cheetah.Auth.Configuration;
using IdentityModel.Client;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Cheetah.Auth.Authentication
{
    /// <summary>
    /// OAuth2 token provider to retrieve OAuth2 tokens.
    /// </summary>
    public class OAuthTokenProvider : ICachableTokenProvider
    {
        /// <summary>
        /// 
        /// </summary>
        internal readonly IHttpClientFactory _httpClientFactory;
        
        private readonly OAuth2Config _config;

        /// <summary>
        /// Creates a new instance of <see cref="OAuthTokenProvider"/>
        /// </summary>
        /// <param name="config">OAuth2 configuration</param>
        /// <param name="httpClientFactory">httpClientFactory to create a httpClient</param>
        public OAuthTokenProvider(OAuth2Config config, IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        /// <summary>
        /// Get a token response asynchronously.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>TokenResponse</returns>
        public async Task<TokenResponse?> GetTokenResponse(CancellationToken cancellationToken)
        {
            using var httpClient = CreateHttpClient();
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual HttpClient CreateHttpClient()
        {
            return _httpClientFactory.CreateClient("OAuthTokenProvider");
        }
    }
}
