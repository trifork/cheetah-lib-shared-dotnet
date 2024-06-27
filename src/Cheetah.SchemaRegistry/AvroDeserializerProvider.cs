using System;
using Cheetah.Kafka.Serdes;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.SchemaRegistry.Avro
{
    /// <summary>
    /// Provides an implementation of <see cref="IDeserializerProvider"/> for Avro deserialization.
    /// </summary>
    public class AvroDeserializerProvider : IDeserializerProvider
    {
        readonly ISchemaRegistryClient _schemaRegistryClient;
        readonly AvroDeserializerConfig? _deserializerConfig;

        /// <summary>
        /// Creates an instance of <see cref="AvroSerializerProvider"/> from services.
        /// </summary>
        /// <param name="deserializerConfig">Optional Avro deserializer configuration.</param>
        /// <returns>A function that creates an <see cref="AvroDeserializerProvider"/> instance.</returns>
        public static Func<IServiceProvider, AvroDeserializerProvider> FromServices(AvroDeserializerConfig? deserializerConfig = null)
        {
            return serviceProvider => new AvroDeserializerProvider(serviceProvider.GetRequiredService<ISchemaRegistryClient>(), deserializerConfig);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AvroDeserializerProvider"/> class.
        /// </summary>
        /// <param name="schemaRegistryClient">The Schema Registry client.</param>
        /// <param name="deserializerConfig">Optional Avro deserializer configuration.</param>
        public AvroDeserializerProvider(ISchemaRegistryClient schemaRegistryClient, AvroDeserializerConfig? deserializerConfig = null)
        {
            _schemaRegistryClient = schemaRegistryClient;
            _deserializerConfig = deserializerConfig;
        }

        /// <summary>
        /// Gets a deserializer for the specified type using Avro deserialization.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize.</typeparam>
        /// <returns>An instance of <see cref="AvroDeserializer{T}"/>.</returns>
        public IDeserializer<T> GetValueDeserializer<T>()
        {
            return new AvroDeserializer<T>(_schemaRegistryClient, _deserializerConfig).AsSyncOverAsync();
        }

        /// <summary>
        /// Gets a deserializer for the specified type using Avro deserialization.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize.</typeparam>
        /// <returns>An instance of <see cref="AvroDeserializer{T}"/>.</returns>
        public IDeserializer<T> GetKeyDeserializer<T>()
        {
            return new AvroDeserializer<T>(_schemaRegistryClient, _deserializerConfig).AsSyncOverAsync();
        }
    }
}
