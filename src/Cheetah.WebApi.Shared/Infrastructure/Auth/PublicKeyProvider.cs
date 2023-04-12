using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Cheetah.WebApi.Shared.Core.Config;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace Cheetah.WebApi.Shared.Infrastructure.Auth;

public class PublicKeyProvider : IPublicKeyProvider
{
    private readonly IOptions<OAuthConfig> _oauthConfig;
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;

    public PublicKeyProvider(IOptions<OAuthConfig> oauthConfig, HttpClient httpClient, IMemoryCache memoryCache)
    {
        _oauthConfig = oauthConfig;
        _httpClient = httpClient;
        _memoryCache = memoryCache;
    }

    public async Task<JsonWebKey[]> GetKey(string clientId)
    {
        var cachedValue = await _memoryCache.GetOrCreateAsync(
            clientId, async cacheEntryFactory =>
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
            });
        return cachedValue;
    }

    public async Task<JsonWebKey[]> GetKeys(string clientId)
    {
        var jwksUri = $"{_oauthConfig.Value.OAuthUrl}/.well-known/jwks.json?clientId={clientId}";

        var response = await _httpClient.GetAsync(jwksUri); //todo: cache?
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        return new JsonWebKeySet(responseString).Keys.ToArray();
    }
}