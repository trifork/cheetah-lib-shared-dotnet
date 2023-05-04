using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Cheetah.WebApi.Shared.Core;
using Cheetah.WebApi.Shared.Core.Config;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Cheetah.WebApi.Shared.Infrastructure.Auth
{
    /// <summary>
    /// Public key provider used to provide public keys
    /// </summary>
    public class PublicKeyProvider : IPublicKeyProvider
    {
        private readonly IOptions<OAuthConfig> _oauthConfig;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;

        public PublicKeyProvider(
            IOptions<OAuthConfig> oauthConfig,
            HttpClient httpClient,
            IMemoryCache memoryCache
        )
        {
            _oauthConfig = oauthConfig;
            _httpClient = httpClient;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Get public key based on client id
        /// </summary>
        /// <returns> returns a JWT public key </returns>
        public async Task<JsonWebKey[]> GetKey(string clientId)
        {
            var cachedValue = await _memoryCache.GetOrCreateAsync(
                clientId,
                async cacheEntryFactory =>
                {
                    var keys = await GetKeys(clientId);
                    if (keys.Any())
                    {
                        cacheEntryFactory.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
                        cacheEntryFactory.SlidingExpiration = TimeSpan.FromMinutes(1); //todo: appsettings
                    }
                    else
                    {
                        cacheEntryFactory.SlidingExpiration = null;
                        cacheEntryFactory.AbsoluteExpirationRelativeToNow = null;
                    }

                    return keys;
                }
            );
            return cachedValue;
        }

        /// <summary>
        /// Get public keys based on client id
        /// </summary>
        /// <returns> returns a JWT public key </returns>
        public async Task<JsonWebKey[]> GetKeys(string clientId)
        {
            var jwksUriBuilder = new UriBuilder(_oauthConfig.Value.OAuthUrl)
            {
                Path = $"/.well-known/jwks.json",
                Query = $"clientId={clientId}"
            };

            var response = await _httpClient.GetAsync(jwksUriBuilder.Uri);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            return new JsonWebKeySet(responseString).Keys.ToArray();
        }
    }
}
