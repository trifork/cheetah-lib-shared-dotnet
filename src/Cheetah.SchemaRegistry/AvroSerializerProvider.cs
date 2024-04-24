using System;
using Cheetah.Kafka.Serialization;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Kafka.Avro
{
    /// <summary>
    /// Provides an implementation of <see cref="ISerializerProvider"/> for Avro serialization.
    /// </summary>
    public class AvroSerializerProvider : ISerializerProvider
    {
        readonly ISchemaRegistryClient _schemaRegistryClient;
        readonly AvroSerializerConfig? _serializerConfig;

        /// <summary>
        /// Creates an instance of <see cref="AvroSerializerProvider"/> from services.
        /// </summary>
        /// <param name="serializerConfig">Optional Avro serializer configuration.</param>
        /// <returns>A function that creates an <see cref="AvroSerializerProvider"/> instance.</returns>
        public static Func<IServiceProvider, AvroSerializerProvider> FromServices(AvroSerializerConfig? serializerConfig = null)
        {
            return serviceProvider => new AvroSerializerProvider(serviceProvider.GetRequiredService<ISchemaRegistryClient>(), serializerConfig);
        }
    
        /// <summary>
        /// Initializes a new instance of the <see cref="AvroSerializerProvider"/> class.
        /// </summary>
        /// <param name="schemaRegistryClient">The Schema Registry client.</param>
        /// <param name="serializerConfig">Optional Avro serializer configuration.</param>
        public AvroSerializerProvider(ISchemaRegistryClient schemaRegistryClient, AvroSerializerConfig? serializerConfig = null)
        {
            _schemaRegistryClient = schemaRegistryClient;
            _serializerConfig = serializerConfig;
        }

        /// <summary>
        /// Gets a serializer for the specified type using Avro serialization.
        /// </summary>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        /// <returns>An instance of <see cref="AvroSerializer{T}"/>.</returns>
        public ISerializer<T> GetSerializer<T>()
        {
            return new AvroSerializer<T>(_schemaRegistryClient, _serializerConfig).AsSyncOverAsync();  
        }

        /// <summary>
        /// Gets a deserializer for the specified type using Avro serialization.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize.</typeparam>
        /// <returns>An instance of <see cref="AvroDeserializer{T}"/>.</returns>
        public IDeserializer<T> GetDeserializer<T>()
        {
            return new AvroDeserializer<T>(_schemaRegistryClient, _serializerConfig).AsSyncOverAsync();
        }
    }
}
