using System;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.SchemaRegistry.Avro
{
    /// <summary>
    /// Provides methods for creating Avro serializers.
    /// </summary>
    public static class AvroSerializer
    {
        /// <summary>
        /// Creates an Avro serializer from services.
        /// </summary>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        /// <param name="serializerConfig">Optional Avro serializer configuration.</param>
        /// <returns>A function that creates an Avro serializer instance.</returns>
        public static Func<IServiceProvider, ISerializer<T>> FromServices<T>(AvroSerializerConfig? serializerConfig = null)
        {
            return serviceProvider =>
            {
                var client = serviceProvider.GetRequiredService<ISchemaRegistryClient>();
                return new AvroSerializer<T>(client, serializerConfig).AsSyncOverAsync();
            };
        }
    }

    /// <summary>
    /// Provides methods for creating Avro deserializers.
    /// </summary>
    public static class AvroDeserializer
    {
        /// <summary>
        /// Creates an Avro deserializer from services.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize.</typeparam>
        /// <param name="deserializerConfig">Optional Avro deserializer configuration.</param>
        /// <returns>A function that creates an Avro deserializer instance.</returns>
        public static Func<IServiceProvider, IDeserializer<T>> FromServices<T>(AvroDeserializerConfig? deserializerConfig = null)
        {
            return serviceProvider =>
            {
                var client = serviceProvider.GetRequiredService<ISchemaRegistryClient>();
                return new AvroDeserializer<T>(client, deserializerConfig).AsSyncOverAsync();
            };
        }
    }
}
