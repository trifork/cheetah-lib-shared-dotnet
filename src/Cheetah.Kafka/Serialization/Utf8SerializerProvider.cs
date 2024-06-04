using System;
using Cheetah.Kafka.Util;
using Confluent.Kafka;

namespace Cheetah.Kafka.Serialization
{
    /// <summary>
    /// Provides UTF-8 serialization for Kafka messages.
    /// </summary>
    public class Utf8SerializerProvider : ISerializerProvider
    {
        /// <summary>
        /// Creates an instance of <see cref="Utf8SerializerProvider"/> from services.
        /// </summary>
        /// <returns>A function that creates an <see cref="Utf8SerializerProvider"/> instance.</returns>
        public static Func<IServiceProvider, ISerializerProvider> FromServices()
        {
            return _ => new Utf8SerializerProvider();
        }

        /// <summary>
        /// Gets a serializer for the specified type using UTF-8 encoding.
        /// </summary>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        /// <returns>An instance of <see cref="Utf8Serializer{T}"/>.</returns>
        public ISerializer<T> GetSerializer<T>()
        {
            return new Utf8Serializer<T>();
        }

        /// <summary>
        /// Gets a deserializer for the specified type using UTF-8 encoding.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize.</typeparam>
        /// <returns>An instance of <see cref="Utf8Serializer{T}"/>.</returns>
        public IDeserializer<T> GetDeserializer<T>()
        {
            return new Utf8Serializer<T>();
        }
    }
}
