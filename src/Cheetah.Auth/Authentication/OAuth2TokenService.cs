using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cheetah.Auth.Configuration;
using IdentityModel.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Auth.Authentication
{
    /// <summary>
    /// Service for retrieving OAuth2 access tokens
    /// </summary>
    public class OAuth2TokenService : ITokenService
    {
        private readonly ILogger<OAuth2TokenService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OAuth2Config _config;
        private readonly IMemoryCache _cache;
        private readonly string _cacheKey;

        /// <summary>
        /// Create a new instance of <see cref="OAuth2TokenService"/>
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{OAuth2TokenService}"/> to use for logging</param>
        /// <param name="httpClientFactory">The <see cref="IHttpClientFactory"/> to use for creating necessary <see cref="HttpClient"/>s</param>
        /// <param name="cache">The <see cref="IMemoryCache"/> to use for token caching</param>
        /// <param name="config">An <see cref="IOptions{OAuth2Config}"/> containing necessary configuration</param>
        /// <param name="cacheKey">The key to use when storing and retrieving tokens from the cache</param>
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
            _config.Validate();
            _cacheKey = cacheKey;
        }

        /// <inheritdoc cref="ITokenService.RequestAccessTokenAsync"/>
        public async Task<(string AccessToken, long Expiration)> RequestAccessTokenAsync(CancellationToken cancellationToken)
        {
            var tokenResponse = await RequestAccessTokenCachedAsync(cancellationToken);

            if (tokenResponse == null || tokenResponse.IsError || tokenResponse.AccessToken == null)
            {
                throw new OAuth2TokenException($"Failed to retrieve access token for  {_config.ClientId}, Error: {tokenResponse?.Error}");
            }

            return (tokenResponse.AccessToken, DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn).ToUnixTimeMilliseconds());
        }

        private async Task<TokenResponse> RequestAccessTokenCachedAsync(
            CancellationToken cancellationToken
        )
        {
            ValidateOAuthConfiguration();

            if (_cache.TryGetValue(_cacheKey, out object? cachedValue))
            {
                return ExtractTokenFromCache(cachedValue);
            }

            return await FetchAndCacheNewToken(cancellationToken);
        }

        private void ValidateOAuthConfiguration()
        {
            if (string.IsNullOrEmpty(_config.ClientId) ||
                string.IsNullOrEmpty(_config.ClientSecret) ||
                string.IsNullOrEmpty(_config.TokenEndpoint))
            {
                throw new ArgumentException("Missing OAuth configuration! Please check environment variables.");
            }
        }

        private TokenResponse ExtractTokenFromCache(object? cachedValue)
        {
            if (cachedValue is TokenResponse tokenResponse)
            {
                _logger.LogDebug("Access token retrieved from cache for {ClientId} with key: {CacheKey}, TokenType: {TokenType}",
                                 _config.ClientId, _cacheKey, tokenResponse.TokenType);
                return tokenResponse;
            }

            throw new OAuth2TokenException("Retrieved access token was null or of an incorrect type.");
        }

        private async Task<TokenResponse> FetchAndCacheNewToken(CancellationToken cancellationToken)
        {
            using var cacheEntry = _cache.CreateEntry(_cacheKey);
            var tokenResponse = await FetchAccessTokenAsync(cancellationToken);
            cacheEntry.AbsoluteExpirationRelativeToNow = GetCacheEntryExpiration(tokenResponse);

            _logger.LogDebug("New access token retrieved for {ClientId} and saved in cache with key: {CacheKey}, TokenType: {TokenType}",
                             _config.ClientId, _cacheKey, tokenResponse.TokenType);
            cacheEntry.SetValue(tokenResponse)
                .RegisterPostEvictionCallback((key, value, reason, state) =>
                {
                    _logger.LogDebug("Access token evicted from cache for {ClientId} with key: {CacheKey}, Reason: {Reason}",
                                     _config.ClientId, _cacheKey, reason);
                });

            return tokenResponse;
        }

        private static TimeSpan GetCacheEntryExpiration(TokenResponse tokenResponse)
        {
            var clockSkew = TimeSpan.FromMinutes(5); // todo: move to setting
            if (tokenResponse.ExpiresIn <= 0)
            {
                throw new OAuth2TokenException("Token response did not contain an expiration time");
            }
            var absoluteExpirationSeconds = Math.Max(10, tokenResponse.ExpiresIn - clockSkew.TotalSeconds);
            return TimeSpan.FromSeconds(absoluteExpirationSeconds);
        }

        private async Task<TokenResponse> FetchAccessTokenAsync(
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
                .RequestClientCredentialsTokenAsync(scope: _config.Scope, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return !tokenResponse.IsError
                ? tokenResponse
                : throw new OAuth2TokenException(tokenResponse.ErrorDescription);
        }
    }
}
