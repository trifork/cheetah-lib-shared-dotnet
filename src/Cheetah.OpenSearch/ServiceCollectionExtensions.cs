using Cheetah.Core.Authentication;
using Cheetah.OpenSearch.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenSearch.Client;

namespace Cheetah.OpenSearch
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCheetahOpenSearch(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddMemoryCache();
            serviceCollection.Configure<OpenSearchConfig>(configuration.GetSection(OpenSearchConfig.Position));
            serviceCollection.AddHttpClient<CheetahOpenSearchTokenService>();
            serviceCollection.AddSingleton<ITokenService, CheetahOpenSearchTokenService>();
            serviceCollection.AddSingleton<OpenSearchClientFactory>();
            serviceCollection.AddSingleton<IOpenSearchClient>(sp => sp.GetRequiredService<OpenSearchClientFactory>().CreateOpenSearchClient());
            
            return serviceCollection;
        }
    }
}
