using System.Net.Http;
using Cheetah.Core;
using Cheetah.Core.Authentication;
using Cheetah.Core.Configuration;
using Cheetah.Kafka.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Kafka.Extensions
{
    /// <summary>
    /// Extension method for adding Cheetah Kafka client factory to IServiceCollection
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers and configures a KafkaClientFactory with the provided configuration for dependency injection, along with its required dependencies.
        /// </summary>
        /// <remarks>
        /// This method requires that the <see cref="KafkaConfig"/> section is configured in the supplied <see cref="IConfiguration"/> instance.
        /// </remarks>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to register the <see cref="KafkaClientFactory"/> and its required services with.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> instance to use for configuration.</param>
        /// <returns>The supplied <see cref="IServiceCollection"/> instance for method chaining.</returns>
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
