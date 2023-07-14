using IdentityModel.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Cheetah.Core.Infrastructure.Auth
{
    public abstract class TokenService : ITokenService
    {
        private readonly ILogger logger;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string? scope;
        private readonly string tokenEndpoint;
        private readonly IMemoryCache cache;

        public abstract string CacheKey { get; }

        public TokenService(
            ILogger logger,
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            string clientId,
            string clientSecret,
            string tokenEndpoint,
            string? scope = null
        )
        {
            this.cache = cache;
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.tokenEndpoint = tokenEndpoint;
            this.scope = scope;
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
                string.IsNullOrEmpty(clientId)
                || string.IsNullOrEmpty(clientSecret)
                || string.IsNullOrEmpty(tokenEndpoint)
            )
            {
                logger.LogError("Missing OAuth config! Please check environment variables");
                return default;
            }
            try
            {

                return await cache.GetOrCreateAsync(
                    CacheKey,
                    async cacheEntry =>
                    {
                        var tokenResponse = await RequestClientCredentialsTokenAsync(cancellationToken);
                        TimeSpan absoluteExpiration = TimeSpan.FromSeconds(
                            Math.Max(10, tokenResponse.ExpiresIn - 10)
                        );
                        cacheEntry.AbsoluteExpirationRelativeToNow = absoluteExpiration;
                        logger.LogDebug(
                            "New access token retrieved for {clientId} and saved in cache with key: {CacheKey}",
                            clientId,
                            CacheKey
                        );
                        return tokenResponse;
                    }
                );
            }
            catch (Exception)
            {
                logger.LogWarning("It's a me - MARIO");
                throw;
            }
        }

        /// <summary>
        /// Request access token with client credentials
        /// </summary>
        /// <returns>Token response</returns>
        public async Task<TokenResponse> RequestClientCredentialsTokenAsync(
            CancellationToken cancellationToken
        )
        {
            if (httpClientFactory == null)
            {
                logger.LogWarning("Http client is null");
                throw new NullReferenceException(nameof(httpClientFactory));
            }
            if (
                string.IsNullOrEmpty(clientId)
                || string.IsNullOrEmpty(clientSecret)
                || string.IsNullOrEmpty(tokenEndpoint)
            )
            {
                logger.LogError("Missing OAuth config! Please check environment variables");
                return new TokenResponse();
            }

            using var httpClient = httpClientFactory.CreateClient(CacheKey);
            var tokenClient = new TokenClient(
                httpClient,
                new TokenClientOptions()
                {
                    Address = tokenEndpoint,
                    ClientId = clientId,
                    ClientSecret = clientSecret
                }
            );
            var tokenResponse = await tokenClient
                .RequestClientCredentialsTokenAsync(cancellationToken: cancellationToken, scope: scope)
                .ConfigureAwait(false);

            // Check if the token request was successful
            if (tokenResponse == null)
            {
                logger.LogWarning("TokenResponse is null");
                throw new NullReferenceException(nameof(tokenResponse));
            }
            return !tokenResponse.IsError ? tokenResponse : throw tokenResponse.Exception; // Get the access token from the token response                
        }
    }
}
