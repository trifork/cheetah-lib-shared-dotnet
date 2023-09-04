using System;
using Cheetah.Core.Infrastructure.Auth;
using Confluent.Kafka;

namespace Cheetah.ComponentTest.Kafka;

internal class KafkaPropsBase
{
    internal string KafkaUrl { get; set; }
    internal string Topic { get; set; }
    internal ITokenService TokenService { get; set; }
}

internal class KafkaWriterProps<TKey, T> : KafkaPropsBase
{
    // This is ac cool comment
    internal ISerializer<T> Serializer { get; set; }
    internal Func<T, TKey> KeyFunction { get; set; }
}

internal class KafkaReaderProps<T> : KafkaPropsBase
{
    internal string ConsumerGroup { get; set; }
    internal IDeserializer<T> Deserializer { get; set; }
}
