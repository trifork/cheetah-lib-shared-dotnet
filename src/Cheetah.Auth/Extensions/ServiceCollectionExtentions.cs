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
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="key"></param>
        /// <param name="oAuthConfig"></param>
        /// <returns></returns>
        public static IServiceCollection AddKeyedTokenService(this IServiceCollection serviceCollection, string key, OAuth2Config oAuthConfig)
        {
            // TODO: Make sure that http clients and their configuration are unique per token service - Unsure whether that's the case right now.
            serviceCollection.AddHttpClient<CachedTokenProvider>(key);
            
            serviceCollection.AddKeyedSingleton<ICachableTokenProvider>(key, (sp, serviceKey) => 
                new OAuthTokenProvider(
                    oAuthConfig,
                    sp.GetRequiredService<IHttpClientFactory>()));
            
            serviceCollection.AddKeyedSingleton<ITokenService>(key, (sp, serviceKey) =>
                new CachedTokenProvider(
                    oAuthConfig,
                    sp.GetRequiredKeyedService<ICachableTokenProvider>(key),
                    sp.GetRequiredService<ILogger<CachedTokenProvider>>()));

            serviceCollection.AddHostedService<StartUpTokenService>(
                sp => new StartUpTokenService(sp.GetRequiredKeyedService<ITokenService>(key))
            );
            return serviceCollection;
        }
    }
}
