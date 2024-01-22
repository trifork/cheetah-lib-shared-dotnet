using System;
using Confluent.Kafka;

namespace Cheetah.Kafka.Serialization
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
        ISerializer<T> GetSerializer<T>();
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IDeserializer<T> GetDeserializer<T>();
        
    }
}
