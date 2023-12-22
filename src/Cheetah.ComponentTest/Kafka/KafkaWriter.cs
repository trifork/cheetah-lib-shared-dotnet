using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cheetah.Auth.Authentication;
using Cheetah.Kafka.Extensions;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Cheetah.ComponentTest.Kafka
{

    public interface IKafkaWriter<T>
    {
        public Task WriteAsync(params T[] messages);
    }
    
    // TODO: Evaluate if we should move KafkaWriter and KafkaReader to Cheetah.Kafka
    public class KafkaWriter<TKey, T> : IKafkaWriter<T>
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
                .AddCheetahOAuthentication(() => props.TokenService.RequestAccessTokenAsync(CancellationToken.None), new LoggerFactory().CreateLogger<OAuth2TokenService>())
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
