using System.Text;
using System.Text.Json;
using Confluent.Kafka;

namespace Cheetah.ComponentTest
{
    public class Utf8Serializer<T> : ISerializer<T>, IDeserializer<T>
    {
        public byte[] Serialize(T data, SerializationContext context)
        {
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
        }

        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            var value = JsonSerializer.Deserialize<T>(data);
            if (value is null)
            {
                throw new ArgumentException(
                    $"Deserialization to type '{typeof(T).Name}' returned a null response"
                );
            }
            return value;
        }
    }
}
