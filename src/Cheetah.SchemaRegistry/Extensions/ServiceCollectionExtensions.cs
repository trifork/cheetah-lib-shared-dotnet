using Cheetah.Auth.Authentication;
using Cheetah.Auth.Configuration;
using Cheetah.Auth.Extensions;
using Cheetah.SchemaRegistry.Avro;
using Cheetah.SchemaRegistry.Configuration;
using Cheetah.SchemaRegistry.Utils;
using Confluent.SchemaRegistry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Cheetah.SchemaRegistry.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring Cheetah Schema Registry.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Cheetah Schema Registry to the service collection.
        /// </summary>
        /// <param name="serviceCollection">The service collection to add the services to.</param>
        /// <param name="configuration">The configuration containing Schema Registry settings.</param>
        public static void AddCheetahSchemaRegistry(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddOptionsWithValidateOnStart<SchemaConfig>()
                .Bind(configuration.GetSection(SchemaConfig.Position));

            var configOAuth = new OAuth2Config();
            configuration.GetSection(SchemaConfig.Position).GetSection(nameof(SchemaConfig.OAuth2)).Bind(configOAuth);
            configOAuth.Validate();

            serviceCollection.TryAddCheetahKeyedTokenService(Constants.TokenServiceKey, configOAuth);

            serviceCollection.AddSingleton<ISchemaRegistryClient>(serviceProvider =>
            {
                var authHeaderValueProvider = new OAuthHeaderValueProvider(serviceProvider.GetRequiredKeyedService<ITokenService>(Constants.TokenServiceKey));
                var schemaConfig = serviceProvider.GetRequiredService<IOptions<SchemaConfig>>().Value;
                return new CachedSchemaRegistryClient(new SchemaRegistryConfig
                {
                    Url = schemaConfig.Url
                }, authHeaderValueProvider);
            });
        }
    }
}
