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

                logger.LogWarning("1");
                return await cache.GetOrCreateAsync(
                    CacheKey,
                    async cacheEntry =>
                    {
                        
                        logger.LogWarning("2");
                        var tokenResponse = await RequestClientCredentialsTokenAsync(cancellationToken);
                        
                        logger.LogWarning("Many");
                        TimeSpan absoluteExpiration = TimeSpan.FromSeconds(
                            Math.Max(10, tokenResponse.ExpiresIn - 10)
                        );
                        
                        logger.LogWarning("Many more");
                        cacheEntry.AbsoluteExpirationRelativeToNow = absoluteExpiration;
                        
                        logger.LogWarning("Many again");
                        logger.LogDebug(
                            "New access token retrieved for {clientId} and saved in cache with key: {CacheKey}",
                            clientId,
                            CacheKey
                        );
                        
                        logger.LogWarning("Many more again");
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
            
            logger.LogWarning("3");
            if (httpClientFactory == null)
            {
                logger.LogWarning("Http client is null");
                throw new NullReferenceException(nameof(httpClientFactory));
            }
            
            logger.LogWarning("4");
            if (
                string.IsNullOrEmpty(clientId)
                || string.IsNullOrEmpty(clientSecret)
                || string.IsNullOrEmpty(tokenEndpoint)
            )
            {
                logger.LogError("Missing OAuth config! Please check environment variables");
                return new TokenResponse();
            }

            
            logger.LogWarning("5");
            using var httpClient = httpClientFactory.CreateClient(CacheKey);
            
            logger.LogWarning("6");
            var tokenClient = new TokenClient(
                httpClient,
                new TokenClientOptions()
                {
                    Address = tokenEndpoint,
                    ClientId = clientId,
                    ClientSecret = clientSecret
                }
            );
            
            logger.LogWarning("7");
            var tokenResponse = await tokenClient
                .RequestClientCredentialsTokenAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            
            logger.LogWarning("8");
            // Check if the token request was successful
            if (tokenResponse == null)
            {
                logger.LogWarning("TokenResponse is null");
                throw new NullReferenceException(nameof(tokenResponse));
            }
            
            logger.LogWarning("9");
            return !tokenResponse.IsError ? tokenResponse : throw tokenResponse.Exception; // Get the access token from the token response                
        }
    }
}
