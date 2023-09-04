using System;
using System.Threading.Tasks;
using Cheetah.Core.Infrastructure.Auth;
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

        internal KafkaWriter(
            string topic,
            Func<T, TKey> keyFunction,
            string url,
            ITokenService tokenService,
            ISerializer<T> serializer
        )
        {
            Topic = topic;
            KeyFunction = keyFunction;
            Logger.LogInformation("Preparing Kafka producer, producing to topic '{topic}'", Topic);

            Producer = new ProducerBuilder<TKey, T>(
                new ProducerConfig
                {
                    BootstrapServers = url,
                    SaslMechanism = SaslMechanism.OAuthBearer,
                    SecurityProtocol = SecurityProtocol.SaslPlaintext,
                })
                .SetValueSerializer(serializer)
                .AddCheetahOAuthentication(tokenService, Logger)
                .Build();
        }

        public async Task WriteAsync(T message)
        {
            var kafkaMessage = new Message<TKey, T>
            {
                Key = KeyFunction(message),
                Value = message
            };
            await Producer.ProduceAsync(Topic, kafkaMessage);
        }

        public async Task WriteAsync(params T[] messages)
        {
            foreach (var message in messages)
            {
                await WriteAsync(message);
            }
        }
    }
}
