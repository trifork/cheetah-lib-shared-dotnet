using Cheetah.Auth.Authentication;
using Cheetah.Auth.Configuration;
using Cheetah.Auth.Extensions;
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
        
            serviceCollection.AddOptionsWithValidateOnStart<OAuth2Config>()
                .Bind(configuration.GetSection(SchemaConfig.Position).GetSection(nameof(SchemaConfig.OAuth2)));
            
            serviceCollection.AddKeyedTokenService("schema-registry");
            
            serviceCollection.AddSingleton<ISchemaRegistryClient>(serviceProvider => 
            {
                var authHeaderValueProvider = new OAuthHeaderValueProvider(serviceProvider.GetRequiredKeyedService<ITokenService>("schema-registry"));
                var schemaConfig = serviceProvider.GetRequiredService<IOptions<SchemaConfig>>().Value;
                return new CachedSchemaRegistryClient(new SchemaRegistryConfig
                {
                    Url = schemaConfig.Url
                }, authHeaderValueProvider);
            });
        }
    }
}
