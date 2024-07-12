using System;
using Cheetah.Auth.Authentication;
using Cheetah.Auth.Configuration;
using Cheetah.Auth.Extensions;
using Cheetah.Kafka.Configuration;
using Cheetah.Kafka.Serdes;
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
        /// <param name="configure">Optional action to configure Kafka behavior</param>
        /// <returns>The supplied <see cref="IServiceCollection"/> instance for method chaining.</returns>
        public static ClientInjector AddCheetahKafka(this IServiceCollection serviceCollection, IConfiguration configuration, Action<ClientFactoryOptions>? configure = null)
        {
            var options = new ClientFactoryOptions();
            configure?.Invoke(options);
            serviceCollection.AddSingleton(options);

            var kafkaConfig = ConfigureAndGetKafkaConfig(serviceCollection, configuration);

            if (kafkaConfig.SaslMechanism == Confluent.Kafka.SaslMechanism.OAuthBearer)
            {
                var configOAuth = new OAuth2Config();
                configuration.GetSection(KafkaConfig.Position).GetSection(nameof(KafkaConfig.OAuth2)).Bind(configOAuth);
                configOAuth.Validate();
                serviceCollection.TryAddCheetahKeyedTokenService(Constants.TokenServiceKey, configOAuth);
            }

            serviceCollection.AddSingleton(options.SerializerProviderFactory);
            serviceCollection.AddSingleton(options.DeserializerProviderFactory);


            serviceCollection.AddSingleton(sp =>
                new KafkaClientFactory(sp.GetKeyedService<ITokenService>(Constants.TokenServiceKey),
                    sp.GetRequiredService<ILoggerFactory>(),
                    sp.GetRequiredService<IOptions<KafkaConfig>>(),
                    sp.GetRequiredService<ClientFactoryOptions>(),
                    sp.GetRequiredService<ISerializerProvider>(),
                    sp.GetRequiredService<IDeserializerProvider>()
                    ));

            return new ClientInjector(serviceCollection);
        }

        private static KafkaConfig ConfigureAndGetKafkaConfig(
            this IServiceCollection serviceCollection,
            IConfiguration configuration
        )
        {
            var config = new KafkaConfig();
            configuration.GetSection(KafkaConfig.Position).Bind(config);
            config.Validate();

            serviceCollection
                .AddOptionsWithValidateOnStart<KafkaConfig>()
                .Bind(configuration.GetSection(KafkaConfig.Position));

            return config;
        }
    }
}
