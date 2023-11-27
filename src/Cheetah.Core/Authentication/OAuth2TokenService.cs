using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Core.Authentication
{
    public class OAuth2TokenService : ITokenService
    {
        private readonly ILogger<OAuth2TokenService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OAuth2Config _config;
        private readonly IMemoryCache _cache;
        private readonly string _cacheKey;

        public OAuth2TokenService(
            ILogger<OAuth2TokenService> logger,
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            IOptions<OAuth2Config> config,
            string cacheKey
        )
        {
            _cache = cache;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _config = config.Value;
            _cacheKey = cacheKey;
        }

        /// <summary>
        /// Request access token
        /// </summary>
        /// <returns>Token response</returns>
        public async Task<TokenResponse?> RequestAccessTokenCachedAsync(
            CancellationToken cancellationToken
        )
        {
            if (
                string.IsNullOrEmpty(_config.ClientId)
                || string.IsNullOrEmpty(_config.ClientSecret)
                || string.IsNullOrEmpty(_config.TokenEndpoint)
            )
            {
                _logger.LogError("Missing OAuth config! Please check environment variables");
                return default;
            }

            return await _cache.GetOrCreateAsync(
                _cacheKey,
                async cacheEntry =>
                {
                    var tokenResponse = await RequestClientCredentialsTokenAsync(cancellationToken);
                    TimeSpan absoluteExpiration = TimeSpan.FromSeconds(
                        Math.Max(10, tokenResponse.ExpiresIn - 10)
                    );

                    cacheEntry.AbsoluteExpirationRelativeToNow = absoluteExpiration;
                    _logger.LogDebug("New access token retrieved for {clientId} and saved in cache with key: {CacheKey}", _config.ClientId, _cacheKey);

                    return tokenResponse;
                }
            );
        }

        /// <summary>
        /// Request access token with client credentials
        /// </summary>
        /// <returns>Token response</returns>
        public async Task<TokenResponse> RequestClientCredentialsTokenAsync(
            CancellationToken cancellationToken
        )
        {
            if (
                string.IsNullOrEmpty(_config.ClientId)
                || string.IsNullOrEmpty(_config.ClientSecret)
                || string.IsNullOrEmpty(_config.TokenEndpoint)
            )
            {
                throw new OAuth2TokenException("Missing OAuth config! Please check environment variables");
            }

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
                .RequestClientCredentialsTokenAsync(scope: _config.AuthScope, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return !tokenResponse.IsError
                ? tokenResponse
                : throw new OAuth2TokenException(tokenResponse.Error);
        }
    }
}
