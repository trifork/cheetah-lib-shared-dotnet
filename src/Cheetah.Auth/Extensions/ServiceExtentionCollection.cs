using System.Net.Http;
using Cheetah.Auth.Authentication;
using Cheetah.Auth.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Auth.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class ServiceExtentionCollection
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="key"></param>
        public static void AddTokenService(IServiceCollection serviceCollection, string key)
        {
            serviceCollection.AddHttpClient<OAuth2TokenService>();
            serviceCollection.AddMemoryCache();
            serviceCollection.AddKeyedSingleton<ITokenService>(key, (sp, serviceKey) =>
                new OAuth2TokenService(
                    sp.GetRequiredKeyedService<ILogger<OAuth2TokenService>>(serviceKey),
                    sp.GetRequiredService<IHttpClientFactory>(),
                    sp.GetRequiredService<IMemoryCache>(),
                    sp.GetRequiredKeyedService<IOptions<OAuth2Config>>(serviceKey),
                    key));
        }
    }
}
