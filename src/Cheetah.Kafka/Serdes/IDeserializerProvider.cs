using Confluent.Kafka;

namespace Cheetah.Kafka.Serdes
{
    /// <summary>
    /// Interface for deserializer provider to be used to deserialize Kafka Messages
    /// </summary>
    public interface IDeserializerProvider
    {
        /// <summary>
        /// Get a value deserializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns><see cref="IDeserializer{T}"/></returns>
        IDeserializer<T> GetValueDeserializer<T>();

        /// <summary>
        /// Get a key deserializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns><see cref="IDeserializer{T}"/></returns>
        IDeserializer<T>? GetKeyDeserializer<T>();
    }
}
