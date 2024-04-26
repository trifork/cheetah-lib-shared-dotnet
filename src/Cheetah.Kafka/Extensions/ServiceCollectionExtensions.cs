using System;
using System.Net.Http;
using Cheetah.Auth.Authentication;
using Cheetah.Auth.Configuration;
using Cheetah.Auth.Extensions;
using Cheetah.Kafka.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;

namespace Cheetah.Kafka.Extensions
{
    /// <summary>
    /// Extension method for adding Cheetah Kafka client factory to IServiceCollection
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Default kafka key used to get required keyed services
        /// </summary>
        const string DefaultKafkaKey = "kafka";
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
        public static CheetahKafkaInjector AddCheetahKafka(
            this IServiceCollection serviceCollection,
            IConfiguration configuration,
            Action<KafkaClientFactoryOptions>? configure = null
        )
        {
            var options = new KafkaClientFactoryOptions();
            configure?.Invoke(options);
            serviceCollection.AddSingleton(options);

            serviceCollection
                .AddOptionsWithValidateOnStart<KafkaConfig>()
                .Bind(configuration.GetSection(KafkaConfig.Position));
            
            var configOAuth = new OAuth2Config();
            configuration.GetSection(KafkaConfig.Position).GetSection(nameof(KafkaConfig.OAuth2)).Bind(configOAuth);
            configOAuth.Validate();

            serviceCollection.AddKeyedTokenService(DefaultKafkaKey, configOAuth);
            
            serviceCollection.AddHostedService<StartUpKafkaTokenService>(
                sp => new StartUpKafkaTokenService(sp.GetRequiredKeyedService<ITokenService>(DefaultKafkaKey))
            );

            serviceCollection.AddSingleton<KafkaClientFactory>(sp =>
                new KafkaClientFactory(sp.GetRequiredKeyedService<ITokenService>(DefaultKafkaKey),
                    sp.GetRequiredService<ILoggerFactory>(),
                    sp.GetRequiredService<IOptions<KafkaConfig>>(),
                    sp.GetRequiredService<KafkaClientFactoryOptions>()));

            return new CheetahKafkaInjector(serviceCollection);
        }
    }
}
