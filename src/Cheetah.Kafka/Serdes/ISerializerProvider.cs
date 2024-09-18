using Confluent.Kafka;

namespace Cheetah.Kafka.Serdes
{
    /// <summary>
    /// Interface for the serializer provider to be used to serialize Kafka Messages
    /// </summary>
    public interface ISerializerProvider
    {
        /// <summary>
        /// Get a value serializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns><see cref="ISerializer{T}"/></returns>
        ISerializer<T> GetValueSerializer<T>();

        /// <summary>
        /// Get a key serializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns><see cref="ISerializer{T}"/></returns>
        ISerializer<T>? GetKeySerializer<T>();
    }
}
