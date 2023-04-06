using IdentityModel.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Cheetah.WebApi.Shared.Infrastructure.Auth
{
    public abstract class TokenService
    {
        private readonly ILogger logger;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string tokenEndpoint;
        private readonly IMemoryCache cache;

        public abstract string CacheKey { get; }

        public TokenService(ILogger logger, IHttpClientFactory httpClientFactory, IMemoryCache cache, string clientId, string clientSecret, string tokenEndpoint)
        {
            this.cache = cache;
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.tokenEndpoint = tokenEndpoint;
        }

        public async Task<string> RequestAccessTokenCachedAsync(CancellationToken cancellationToken)
        {
            return await this.cache.GetOrCreateAsync(CacheKey, async cacheEntry =>
                                    {
                                        var tokenResponse = await RequestClientCredentialsTokenAsync(cancellationToken);
                                        TimeSpan absoluteExpiration = TimeSpan.FromSeconds(Math.Max(10, tokenResponse.ExpiresIn - 10));
                                        cacheEntry.AbsoluteExpirationRelativeToNow = absoluteExpiration;
                                        logger.LogDebug("New access token retrieved for {clientId} saved in cache: {CacheKey}", clientId, CacheKey);
                                        return tokenResponse.AccessToken;
                                    });

        }
        public async Task<TokenResponse> RequestClientCredentialsTokenAsync(CancellationToken cancellationToken)
        {
            using var httpClient = httpClientFactory.CreateClient(CacheKey);
            var tokenClient = new TokenClient(httpClient, new TokenClientOptions()
            {
                Address = tokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret
            });
            var tokenResponse = await tokenClient.RequestClientCredentialsTokenAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            // Check if the token request was successful
            if (!tokenResponse.IsError)
            {
                // Get the access token from the token response
                return tokenResponse;
            }
            else
            {
                throw tokenResponse.Exception;
            }
        }
    }
}