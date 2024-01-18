using Cheetah.Auth.Authentication;
using Cheetah.SchemaRegistry.Configuration;
using Cheetah.SchemaRegistry.Utils;
using Confluent.SchemaRegistry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.SchemaRegistry;

public class CheetahSchemaRegistryClient : CachedSchemaRegistryClient
{
    public CheetahSchemaRegistryClient(
        [FromKeyedServices("SchemaRegistry")] ITokenService tokenService, 
        IOptions<SchemaConfig> config) 
        : base(config.Value.GetSchemaRegistryConfig(), new OAuthHeaderValueProvider(tokenService))
    {
    }
}

