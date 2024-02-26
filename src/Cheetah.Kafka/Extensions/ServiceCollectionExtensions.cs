using System;
using Cheetah.Auth.Configuration;
using Cheetah.Kafka.Configuration;
using Cheetah.Kafka.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static Cheetah.Auth.Extensions.ServiceCollectionExtensions;

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
        /// <param name="configure">Optional action to configure Kafka behavior</param>
        /// <returns>The supplied <see cref="IServiceCollection"/> instance for method chaining.</returns>
        public static ClientInjector AddCheetahKafka(this IServiceCollection serviceCollection, IConfiguration configuration, Action<ClientFactoryOptions>? configure = null)
        {
            var options = new ClientFactoryOptions();
            configure?.Invoke(options);
            serviceCollection.AddSingleton(options);

            serviceCollection
                .AddOptionsWithValidateOnStart<KafkaConfig>()
                .Bind(configuration.GetSection(KafkaConfig.Position));
            
            serviceCollection.AddOptionsWithValidateOnStart<OAuth2Config>()
                .Bind(configuration.GetSection(KafkaConfig.Position).GetSection(nameof(KafkaConfig.OAuth2)));

            serviceCollection.AddKeyedTokenService(Constants.TokenServiceKey);

            serviceCollection.AddSingleton<ClientFactoryOptions>(options);
            serviceCollection.AddSingleton<ISerializerProvider>(options.SerializerProviderFactory);
            serviceCollection.AddSingleton<KafkaClientFactory>();

            return new ClientInjector(serviceCollection);
        }
    }
}
