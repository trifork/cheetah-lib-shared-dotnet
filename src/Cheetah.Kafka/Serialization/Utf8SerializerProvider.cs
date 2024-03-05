using System;
using Cheetah.Kafka.Util;
using Confluent.Kafka;

namespace Cheetah.Kafka.Serialization
{
    /// <summary>
    /// 
    /// </summary>
    public class Utf8SerializerProvider : ISerializerProvider
    {
        public static Func<IServiceProvider, ISerializerProvider> FromServices()
        {
            return _ => new Utf8SerializerProvider();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ISerializer<T> GetSerializer<T>()
        {
            return new Utf8Serializer<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IDeserializer<T> GetDeserializer<T>()
        {
            return new Utf8Serializer<T>();
        }
    }
}
