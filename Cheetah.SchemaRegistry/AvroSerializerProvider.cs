using System;
using Cheetah.Kafka.Serialization;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Kafka.Avro;

public class AvroSerializerProvider : ISerializerProvider
{
    readonly ISchemaRegistryClient _schemaRegistryClient;
    readonly AvroSerializerConfig? _serializerConfig;

    public static Func<IServiceProvider, AvroSerializerProvider> FromServices(AvroSerializerConfig? serializerConfig = null)
    {
        return serviceProvider => new AvroSerializerProvider(serviceProvider.GetRequiredService<ISchemaRegistryClient>(), serializerConfig);
    }
    
    public AvroSerializerProvider(ISchemaRegistryClient schemaRegistryClient, AvroSerializerConfig? serializerConfig = null)
    {
        _schemaRegistryClient = schemaRegistryClient;
        _serializerConfig = serializerConfig;
    }

    public ISerializer<T> GetSerializer<T>(IServiceProvider serviceProvider)
    {
        return new AvroSerializer<T>(_schemaRegistryClient, _serializerConfig).AsSyncOverAsync();  
    }

    public IDeserializer<T> GetDeserializer<T>(IServiceProvider serviceProvider)
    {
        return new AvroDeserializer<T>(_schemaRegistryClient, _serializerConfig).AsSyncOverAsync();
    }
}
