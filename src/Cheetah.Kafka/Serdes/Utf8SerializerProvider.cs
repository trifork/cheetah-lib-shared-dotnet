using System;
using Cheetah.Kafka.Util;
using Confluent.Kafka;

namespace Cheetah.Kafka.Serdes
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
        /// <returns>An instance of <see cref="Utf8JsonSerializer{T}"/>.</returns>
        public ISerializer<T> GetValueSerializer<T>()
        {
            return new Utf8JsonSerializer<T>();
        }

        /// <summary>
        /// Gets a serializer for the specified type using UTF-8 encoding.
        /// </summary>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        /// <returns>An instance of <see cref="Utf8JsonSerializer{T}"/>.</returns>
        public ISerializer<T> GetKeySerializer<T>()
        {
            return new Utf8JsonSerializer<T>();
        }
    }
}
