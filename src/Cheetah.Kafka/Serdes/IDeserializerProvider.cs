using Confluent.Kafka;

namespace Cheetah.Kafka.Serdes
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDeserializerProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IDeserializer<T> GetValueDeserializer<T>();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IDeserializer<T>? GetKeyDeserializer<T>();
    }
}
