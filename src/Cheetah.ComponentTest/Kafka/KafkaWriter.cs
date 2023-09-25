using System;
using System.Linq;
using System.Threading.Tasks;
using Cheetah.Core.Infrastructure.Services.Kafka;
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
                new ProducerConfig {
                    BootstrapServers = props.KafkaUrl,
                    SaslMechanism = SaslMechanism.OAuthBearer,
                    SecurityProtocol = SecurityProtocol.SaslPlaintext,
                })
                .SetValueSerializer(props.Serializer)
                .AddCheetahOAuthentication(props.TokenService, Logger)
                .Build();
        }
        
        /// <summary>
        /// Asynchronously publishes a single message to Kafka
        /// </summary>
        /// <param name="message">The message to publish</param>
        public async Task WriteAsync(T message)
        {
            var kafkaMessage = new Message<TKey, T>
            {
                Key = KeyFunction(message),
                Value = message
            };
            await Producer.ProduceAsync(Topic, kafkaMessage);
        }

        /// <summary>
        /// Asynchronously publishes multiple messages to Kafka
        /// </summary>
        /// <param name="messages">The collection of messages to publish</param>
        /// <exception cref="ArgumentException">Thrown if the provided collection of messages is empty</exception>
        public async Task WriteAsync(params T[] messages)
        {
            if (!messages.Any())
            {
                throw new ArgumentException("WriteAsync was invoked with an empty list of messages.");
            }
            
            foreach (var message in messages)
            {
                await WriteAsync(message);
            }
        }
    }
}
