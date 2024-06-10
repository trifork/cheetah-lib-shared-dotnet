using System;
using Cheetah.Auth.Configuration;
using Cheetah.Auth.Extensions;
using Cheetah.OpenSearch.Configuration;
using Cheetah.OpenSearch.Connection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        /// <param name="configureClientOptions">Optional action used to configure the generated OpenSearchClients</param>
        /// <returns>The supplied <see cref="IServiceCollection"/> instance for method chaining.</returns>
        public static IServiceCollection AddCheetahOpenSearch(
            this IServiceCollection serviceCollection,
            IConfiguration configuration,
            Action<OpenSearchClientOptions>? configureClientOptions = null
        )
        {
            var config = serviceCollection.ConfigureAndGetOpenSearchConfig(configuration);

            // Avoid DI'ing OAuth2 specific services if we're not using OAuth2
            if (config.AuthMode == OpenSearchConfig.OpenSearchAuthMode.OAuth2)
            {
                serviceCollection.AddCheetahOpenSearchOAuth2Connection(configuration);
            }

            var clientOptions = new OpenSearchClientOptions();
            configureClientOptions?.Invoke(clientOptions);

            serviceCollection
                .AddSingleton(ConnectionPoolHelper.GetConnectionPool(config.Url))
                .AddSingleton(clientOptions)
                .AddSingleton<OpenSearchClientFactory>()
                .AddSingleton<IOpenSearchClient>(sp =>
                    sp.GetRequiredService<OpenSearchClientFactory>().CreateOpenSearchClient()
                );

            return serviceCollection;
        }

        static IServiceCollection AddCheetahOpenSearchOAuth2Connection(
            this IServiceCollection serviceCollection, IConfiguration configuration
        )
        {
            var configOAuth = new OAuth2Config();
            configuration.GetSection(OpenSearchConfig.Position).GetSection(nameof(OpenSearchConfig.OAuth2)).Bind(configOAuth);
            configOAuth.Validate();

            serviceCollection.TryAddCheetahKeyedTokenService(Constants.TokenServiceKey, configOAuth);
            serviceCollection.AddSingleton<IConnection, CheetahOpenSearchConnection>();
            return serviceCollection;
        }

        private static OpenSearchConfig ConfigureAndGetOpenSearchConfig(
            this IServiceCollection serviceCollection,
            IConfiguration configuration
        )
        {
            var config = new OpenSearchConfig();
            configuration.GetSection(OpenSearchConfig.Position).Bind(config);
            config.Validate();

            serviceCollection
                .AddOptionsWithValidateOnStart<OpenSearchConfig>()
                .Bind(configuration.GetSection(OpenSearchConfig.Position));

            return config;
        }
    }
}
