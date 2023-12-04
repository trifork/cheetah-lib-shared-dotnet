using System.Net.Http;
using Cheetah.Auth.Authentication;
using Cheetah.Auth.Configuration;
using Cheetah.OpenSearch.Configuration;
using Cheetah.OpenSearch.Connection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using OpenSearch.Net;

namespace Cheetah.OpenSearch.Extensions
{
    /// <summary>
    /// OpenSearch extension methods for <see cref="IServiceCollection"/>
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers and configures an <see cref="IOpenSearchClient"/> for dependency injection, along with its required services.
        /// </summary>
        /// <remarks>
        /// This method requires that the <see cref="OpenSearchConfig"/> section is configured in the supplied <see cref="IConfiguration"/> instance.
        /// </remarks>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to register the <see cref="IOpenSearchClient"/> and its required services with.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> instance to use for configuration.</param>
        /// <returns>The supplied <see cref="IServiceCollection"/> instance for method chaining.</returns>
        public static IServiceCollection AddCheetahOpenSearch(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var config = serviceCollection.ConfigureAndGetOpenSearchConfig(configuration);

            // Avoid DI'ing OAuth2 specific services if we're not using OAuth2
            if (config.AuthMode == OpenSearchConfig.OpenSearchAuthMode.OAuth2)
            {
                serviceCollection.AddCheetahOpenSearchOAuth2Connection();
            }

            serviceCollection
                .AddSingleton<IConnectionPool>(ConnectionPoolHelper.GetConnectionPool(config.Url))
                .AddSingleton<OpenSearchClientFactory>()
                .AddSingleton<IOpenSearchClient>(sp => 
                    sp.GetRequiredService<OpenSearchClientFactory>().CreateOpenSearchClient());
            
            return serviceCollection;
        }

        internal static IServiceCollection AddCheetahOpenSearchOAuth2Connection(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddHttpClient<OAuth2TokenService>();
            serviceCollection.AddMemoryCache();
            serviceCollection.AddSingleton<ITokenService>(sp =>
                    new OAuth2TokenService(
                        sp.GetRequiredService<ILogger<OAuth2TokenService>>(),
                        sp.GetRequiredService<IHttpClientFactory>(),
                        sp.GetRequiredService<IMemoryCache>(),
                        sp.GetRequiredService<IOptions<OAuth2Config>>(), 
                        "opensearch-access-token"));
            serviceCollection.AddSingleton<IConnection, CheetahOpenSearchConnection>();
            return serviceCollection;
        }
        
        private static OpenSearchConfig ConfigureAndGetOpenSearchConfig(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var config = new OpenSearchConfig();
            configuration.GetSection(OpenSearchConfig.Position).Bind(config);
            serviceCollection.AddOptionsWithValidateOnStart<OpenSearchConfig>()
                .Bind(configuration.GetSection(OpenSearchConfig.Position));
            serviceCollection.AddOptionsWithValidateOnStart<OAuth2Config>()
                .Bind(configuration.GetSection(OpenSearchConfig.Position).GetSection(nameof(OpenSearchConfig.OAuth2)));
            
            return config;
        }
    }
}
