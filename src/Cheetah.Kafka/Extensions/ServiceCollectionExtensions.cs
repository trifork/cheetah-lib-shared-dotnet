using System.Net.Http;
using Cheetah.Core;
using Cheetah.Core.Authentication;
using Cheetah.Core.Configuration;
using Cheetah.Kafka.Config;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Kafka.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCheetahKafkaClientFactory(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.Configure<KafkaConfig>(configuration.GetSection(KafkaConfig.Position));
            serviceCollection.Configure<OAuth2Config>(configuration.GetSection(KafkaConfig.Position));
            serviceCollection.AddHttpClient<OAuth2TokenService>();
            serviceCollection.AddMemoryCache();
            serviceCollection.AddSingleton<ITokenService>(sp =>
                new OAuth2TokenService(
                    sp.GetRequiredService<ILogger<OAuth2TokenService>>(),
                    sp.GetRequiredService<IHttpClientFactory>(),
                    sp.GetRequiredService<IMemoryCache>(),
                    sp.GetRequiredService<IOptions<OAuth2Config>>(), 
                    "kafka-access-token"));
            serviceCollection.AddSingleton<KafkaClientFactory>();
            return serviceCollection;
        }
    }
}
