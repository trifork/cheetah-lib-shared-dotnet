using System;
using Cheetah.Kafka.Util;
using Confluent.Kafka;

namespace Cheetah.Kafka.Serdes
{
    /// <summary>
    /// Provides UTF-8 serialization for Kafka messages.
    /// </summary>
    public class Utf8DeserializerProvider : IDeserializerProvider
    {
        /// <summary>
        /// Creates an instance of <see cref="Utf8DeserializerProvider"/> from services.
        /// </summary>
        /// <returns>A function that creates an <see cref="Utf8DeserializerProvider"/> instance.</returns>
        public static Func<IServiceProvider, IDeserializerProvider> FromServices()
        {
            return _ => new Utf8DeserializerProvider();
        }

        /// <summary>
        /// Gets a deserializer for the specified type using UTF-8 encoding.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize.</typeparam>
        /// <returns>An instance of <see cref="Utf8JsonDeserializer{T}"/>.</returns>
        public IDeserializer<T> GetValueDeserializer<T>()
        {
            return new Utf8JsonDeserializer<T>();
        }

        /// <summary>
        /// Gets a deserializer for the specified type using UTF-8 encoding.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize.</typeparam>
        /// <returns>An instance of <see cref="Utf8JsonDeserializer{T}"/>.</returns>
        public IDeserializer<T> GetKeyDeserializer<T>()
        {
            return new Utf8JsonDeserializer<T>();
        }
    }
}
