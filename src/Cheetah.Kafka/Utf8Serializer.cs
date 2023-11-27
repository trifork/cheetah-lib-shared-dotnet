using System;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;

namespace Cheetah.Kafka
{
    /// <summary>
    /// Serializer which serializes data with UTF8-encoding
    /// </summary>
    /// <typeparam name="T">The type to (de)serialize</typeparam>
    public class Utf8Serializer<T> : ISerializer<T>, IDeserializer<T>
    {
        /// <summary>
        /// Serializes the input data using System.Text.Json and UTF8-encoding
        /// </summary>
        /// <param name="data">The data to serialize</param>
        /// <param name="context">The current serialization context</param>
        /// <returns>The serialized data as a byte-array</returns>
        public byte[] Serialize(T data, SerializationContext context)
        {
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
        }

        /// <summary>
        /// Deserializes the input data
        /// </summary>
        /// <param name="data">The data to deserialize</param>
        /// <param name="isNull">Whether the data is null</param>
        /// <param name="context">The current serialization context</param>
        /// <returns>A <typeparamref name="T"/> instance obtained from deserializing the input <paramref name="data"/>.</returns>
        /// <exception cref="ArgumentException"></exception>
        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            var value = JsonSerializer.Deserialize<T>(data);
            if (value is null)
            {
                throw new JsonException(
                    $"Deserialization to type '{typeof(T).Name}' returned a null response"
                );
            }
            return value;
        }
    }
}
