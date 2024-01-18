using Cheetah.Auth.Authentication;
using Cheetah.Kafka;
using Cheetah.Kafka.Configuration;
using Cheetah.SchemaRegistry.Configuration;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.SchemaRegistry;

public class AvroSerializerFactory : ISerializerFactory
{
    readonly SchemaConfig _config;
    readonly ISchemaRegistryClient _schemaRegistryClient;

    public static ISerializerFactory GetFromServices(IServiceProvider provider)
    {
        return new AvroSerializerFactory(provider.GetRequiredService<ISchemaRegistryClient>());
    }
    
    private AvroSerializerFactory(ISchemaRegistryClient schemaRegistryClient)
    {
        _schemaRegistryClient = schemaRegistryClient;
    }
    public ISerializer<T> GetSerializer<T>()
    {
        return new AvroSerializer<T>(_schemaRegistryClient).AsSyncOverAsync();
    }

    public IDeserializer<T> GetDeserializer<T>()
    {
        return new AvroDeserializer<T>(_schemaRegistryClient).AsSyncOverAsync();
    }
}
