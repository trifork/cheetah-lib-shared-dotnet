using System;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Kafka.Avro;

public static class AvroSerializer
{
    public static Func<IServiceProvider, ISerializer<T>> FromServices<T>(AvroSerializerConfig? serializerConfig = null)
    {
        return serviceProvider => {
            var client = serviceProvider.GetRequiredService<ISchemaRegistryClient>();
            return new AvroSerializer<T>(client, serializerConfig).AsSyncOverAsync();
        };
    }
}

public static class AvroDeserializer
{
    public static Func<IServiceProvider, IDeserializer<T>> FromServices<T>(AvroSerializerConfig? serializerConfig = null)
    {
        return serviceProvider =>
        {
            var client = serviceProvider.GetRequiredService<ISchemaRegistryClient>();
            return new AvroDeserializer<T>(client).AsSyncOverAsync();
        };
    }
}
