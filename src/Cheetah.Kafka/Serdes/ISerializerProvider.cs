using Confluent.Kafka;

namespace Cheetah.Kafka.Serdes
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISerializerProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ISerializer<T> GetValueSerializer<T>();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ISerializer<T>? GetKeySerializer<T>();
    }
}
