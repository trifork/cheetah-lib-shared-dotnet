using System;
using System.Threading.Tasks;
using Cheetah.ComponentTest.TokenService;
using Cheetah.Core.Infrastructure.Services.Kafka;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Cheetah.ComponentTest.Kafka
{
    public class KafkaWriter<TKey, T>
    {
        private static readonly ILogger Logger = new LoggerFactory().CreateLogger<KafkaWriter<TKey, T>>();

        internal string? Topic { get; set; }
        internal string? Server { get; set; }
        internal string? ClientId { get; set; }
        internal string? ClientSecret { get; set; }
        internal string? AuthEndpoint { get; set; }
        internal Func<T, TKey>? KeyFunction { get; set; }
        private IProducer<TKey, T>? Producer { get; set; }

        internal KafkaWriter() { }

        internal void Prepare()
        {
            Logger.LogInformation("Preparing kafka producer, producing to topic '{topic}'", Topic);
            Producer = new ProducerBuilder<TKey, T>(new ProducerConfig
            {
                BootstrapServers = Server,
                SaslMechanism = SaslMechanism.OAuthBearer,
                SecurityProtocol = SecurityProtocol.SaslPlaintext,
            })
            .SetValueSerializer(new Utf8Serializer<T>())
            .AddCheetahOAuthentication(new TestTokenService(ClientId, ClientSecret, AuthEndpoint), Logger)
            .Build();
        }

        public void Write(T message)
        {
            var kafkaMessage = new Message<TKey, T>
            {
                Key = KeyFunction(message),
                Value = message
            };
            Producer.Produce(Topic, kafkaMessage);
            Producer.Flush();

        }

        public void Write(params T[] messages)
        {
            foreach (var message in messages)
            {
                var kafkaMessage = new Message<TKey, T>
                {
                    Key = KeyFunction(message),
                    Value = message
                };
                Producer.Produce(Topic, kafkaMessage);
            }
            Producer.Flush();
        }
    }
}
