using System.Net.Http;
using Cheetah.Core;
using Cheetah.Core.Authentication;
using Cheetah.OpenSearch.Config;
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
    public static class ServiceCollectionExtensions
    {
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
            config.ValidateConfig();
            serviceCollection.Configure<OpenSearchConfig>(configuration.GetSection(OpenSearchConfig.Position));
            serviceCollection.Configure<OAuth2Config>(configuration.GetSection(OpenSearchConfig.Position));
            
            return config;
        }
    }
}
