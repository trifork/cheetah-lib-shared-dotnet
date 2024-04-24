using Cheetah.Auth.Authentication;
using Cheetah.Auth.Configuration;
using Cheetah.Auth.Extensions;
using Cheetah.Kafka.Avro;
using Cheetah.SchemaRegistry.Configuration;
using Cheetah.SchemaRegistry.Utils;
using Confluent.SchemaRegistry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cheetah.SchemaRegistry.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCheetahSchemaRegistry(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddOptionsWithValidateOnStart<SchemaConfig>()
                .Bind(configuration.GetSection(SchemaConfig.Position));
        
            var configOAuth = new OAuth2Config();
            configuration.GetSection(SchemaConfig.Position).GetSection(nameof(SchemaConfig.OAuth2)).Bind(configOAuth);
            configOAuth.Validate();
            
            serviceCollection.AddKeyedTokenService(Constants.TokenServiceKey, configOAuth);
            
            serviceCollection.AddHostedService<StartUpSchemaRegistryTokenService>(
                sp => new StartUpSchemaRegistryTokenService(sp.GetRequiredKeyedService<ITokenService>(Constants.TokenServiceKey))
            );
            
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
