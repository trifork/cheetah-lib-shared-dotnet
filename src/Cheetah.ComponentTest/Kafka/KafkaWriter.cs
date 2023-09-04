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
            Topic = props.Topic;
            KeyFunction = props.KeyFunction;
            Logger.LogInformation("Preparing Kafka producer, producing to topic '{topic}'", Topic);
            
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
        
        public async Task WriteAsync(T message)
        {
            if (KeyFunction == null)
            {
                throw new InvalidOperationException("KeyFunction must be set");
            }
            var kafkaMessage = new Message<TKey, T>
            {
                Key = KeyFunction(message),
                Value = message
            };
            await Producer.ProduceAsync(Topic, kafkaMessage);
        }

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
