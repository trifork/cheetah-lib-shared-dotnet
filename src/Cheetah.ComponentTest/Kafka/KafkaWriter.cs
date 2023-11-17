using System;
using System.Linq;
using System.Threading.Tasks;
using Cheetah.Kafka;
using Cheetah.Kafka.Extensions;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Cheetah.ComponentTest.Kafka
{
    public class KafkaWriter<TKey, T>
    {
        private static readonly ILogger Logger = new LoggerFactory().CreateLogger<KafkaWriter<TKey, T>>();

        internal string Topic { get; }
        internal Func<T, TKey> KeyFunction { get; }
        private IProducer<TKey, T> Producer { get; }

        internal KafkaWriter(KafkaWriterProps<TKey, T> props)
        {
            Topic = !string.IsNullOrWhiteSpace(props.Topic)
                ? props.Topic
                : throw new ArgumentException("Topic must not be null or empty");

            KeyFunction = props.KeyFunction ?? throw new ArgumentException("KeyFunction cannot be null");
            Producer = new ProducerBuilder<TKey, T>(
                new ProducerConfig
                {
                    BootstrapServers = props.KafkaUrl,
                    SaslMechanism = SaslMechanism.OAuthBearer,
                    SecurityProtocol = SecurityProtocol.SaslPlaintext,
                })
                .SetValueSerializer(props.Serializer)
                .AddCheetahOAuthentication(props.TokenService, new LoggerFactory().CreateLogger<CheetahKafkaTokenService>())
                .Build();
        }

        /// <summary>
        /// Publishes multiple messages to Kafka
        /// </summary>
        /// <param name="messages">The collection of messages to publish</param>
        /// <exception cref="ArgumentException">Thrown if the provided collection of messages is empty</exception>
        public Task WriteAsync(params T[] messages)
        {
            if (messages.Length == 0)
            {
                throw new ArgumentException("WriteAsync was invoked with an empty list of messages.");
            }

            var kafkaMessages = messages.Select(message => new Message<TKey, T>
            {
                Key = KeyFunction(message),
                Value = message,
            });
            var produceTasks = kafkaMessages.Select(kafkaMessage => Producer.ProduceAsync(Topic, kafkaMessage));
            return Task.WhenAll(produceTasks);
        }
    }
}
