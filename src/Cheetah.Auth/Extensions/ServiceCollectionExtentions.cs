using System.Linq;
using System.Net.Http;
using Cheetah.Auth.Authentication;
using Cheetah.Auth.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cheetah.Auth.Extensions
{
    /// <summary>
    /// Extension method for adding Cheetah auth keyed token service to IServiceCollection
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers and configures a CachedTokenProvider and a TokenService with dependency injection,
        /// utilizing a unique key for GetRequiredKeyedService, ensuring that a distinct singleton is registered for each KeyedTokenService instance.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="key">The key used for resolving the services.</param>
        /// <param name="oAuthConfig">The OAuth2 configuration.</param>
        /// <returns>The modified <see cref="IServiceCollection"/> instance.</returns>
        public static IServiceCollection TryAddCheetahKeyedTokenService(this IServiceCollection serviceCollection, string key, OAuth2Config oAuthConfig)
        {
            if (serviceCollection.Any(s => s.IsKeyedService && s.ServiceKey is string serviceKey && serviceKey == key && typeof(ITokenService).IsAssignableTo(s.ServiceType)))
            {
                return serviceCollection;
            }

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

            serviceCollection.AddSingleton<IHostedService, StartUpTokenService>(sp => new StartUpTokenService(sp.GetRequiredKeyedService<ITokenService>(key)));

            return serviceCollection;
        }
    }
}
