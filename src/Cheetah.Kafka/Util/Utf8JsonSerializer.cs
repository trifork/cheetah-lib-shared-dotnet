using System.Text;
using Newtonsoft.Json;
using Confluent.Kafka;

namespace Cheetah.Kafka.Util
{
    /// <summary>
    /// Serializer which serializes data with UTF8-encoding
    /// </summary>
    /// <typeparam name="T">The type to serialize</typeparam>
    public class Utf8JsonSerializer<T> : ISerializer<T>
    {
        /// <summary>
        /// Serializes the input data using System.Text.Json and UTF8-encoding
        /// </summary>
        /// <param name="data">The data to serialize</param>
        /// <param name="context">The current serialization context</param>
        /// <returns>The serialized data as a byte-array</returns>
        public byte[] Serialize(T data, SerializationContext context)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data, DefaultSerializerSettings.GetDefaultSettings()));
        }
    }
}
