using System;
using Newtonsoft.Json;
using Confluent.Kafka;

namespace Cheetah.Kafka.Util
{
    /// <summary>
    /// Serializer which serializes data with UTF8-encoding
    /// </summary>
    /// <typeparam name="T">The type to (de)serialize</typeparam>
    public class Utf8JsonDeserializer<T> : IDeserializer<T>
    {
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
            return JsonConvert.DeserializeObject<T>(System.Text.Encoding.UTF8.GetString(data.ToArray())) ?? throw new JsonException(
                    $"Deserialization to type '{typeof(T).Name}' returned a null response"
                );
        }
    }
}
