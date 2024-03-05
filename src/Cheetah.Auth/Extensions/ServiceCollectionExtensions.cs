using System.Net.Http;
using Cheetah.Auth.Authentication;
using Cheetah.Auth.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Auth.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKeyedTokenService(this IServiceCollection serviceCollection, string key)
        {
            // TODO: Make sure that http clients and their configuration are unique per token service - Unsure whether that's the case right now.
            serviceCollection.AddHttpClient<OAuth2TokenService>();
            serviceCollection.AddMemoryCache();
            serviceCollection.AddKeyedSingleton<ITokenService>(key, (sp, serviceKey) =>
                new OAuth2TokenService(
                    sp.GetRequiredService<ILogger<OAuth2TokenService>>(),
                    sp.GetRequiredService<IHttpClientFactory>(),
                    sp.GetRequiredService<IMemoryCache>(),
                    sp.GetRequiredService<IOptions<OAuth2Config>>(),
                    key));
            
            return serviceCollection;
        }
    }
}
