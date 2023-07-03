using System;
using System.Collections.Generic;
using System.Threading;
using Cheetah.ComponentTest.TokenService;
using Cheetah.Core.Infrastructure.Services.Kafka;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Cheetah.ComponentTest.Kafka
{
    public class KafkaReader<TKey, T>
    {
        private static readonly ILogger Logger = new LoggerFactory().CreateLogger<KafkaReader<TKey, T>>();

        internal string? Topic { get; set; }
        internal string? Server { get; set; }
        internal string? ClientId { get; set; }
        internal string? ClientSecret { get; set; }
        internal string? AuthEndpoint { get; set; }
        internal IDeserializer<T> Serializer { get; set; } = new Utf8Serializer<T>();
        private IConsumer<TKey, T>? Consumer { get; set; }
        internal string? ConsumerGroup { get; set; }

        internal KafkaReader() { }

        internal void Prepare()
        {
            Logger.LogInformation("Preparing kafka producer, producing to topic '{Topic}'", Topic);
            if (ClientId == null || ClientSecret == null || AuthEndpoint == null)
            {
                throw new InvalidOperationException("ClientId, ClientSecret and AuthEndpoint must be set");
            }
            Consumer = new ConsumerBuilder<TKey, T>(new ConsumerConfig
            {
                BootstrapServers = Server,
                SaslMechanism = SaslMechanism.OAuthBearer,
                SecurityProtocol = SecurityProtocol.SaslPlaintext,
                EnablePartitionEof = true,
                GroupId = ConsumerGroup,
                AllowAutoCreateTopics = true,
                AutoOffsetReset = AutoOffsetReset.Latest
            })
            .SetValueDeserializer(Serializer)
            .AddCheetahOAuthentication(new TestTokenService(ClientId, ClientSecret, AuthEndpoint), Logger)
            .Build();
            Consumer.Assign(new TopicPartition(Topic, 0));
            CancellationTokenSource cancellationTokenSource = new();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(2));
            var cancellationToken = cancellationTokenSource.Token;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = Consumer.Consume(cancellationToken);
                    if (consumeResult.IsPartitionEOF)
                    {
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        public IEnumerable<T> ReadMessages(int count, TimeSpan timeout)
        {
            var messages = new List<T>();

            CancellationTokenSource cancellationTokenSource = new();
            cancellationTokenSource.CancelAfter(timeout);
            var cancellationToken = cancellationTokenSource.Token;
            Log.Information(
                "Consuming messages from '{topic}', expecting a total of {count} messages...",
                Topic,
                count
            );
            while (
                messages.Count < count && !cancellationToken.IsCancellationRequested
            )
            {
                try
                {
                    var consumeResult = Consumer!.Consume(cancellationToken);
                    if (consumeResult.IsPartitionEOF)
                    {
                        continue;
                    }

                    messages.Add(consumeResult.Message.Value);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            if (messages.Count < count)
            {
                throw new InvalidOperationException($"Could not read enough messages from {Topic}, read {messages.Count}, expected {count}");
            }
            return messages;
        }

        public bool VerifyNoMoreMessages(TimeSpan timeout)
        {
            CancellationTokenSource cancellationTokenSource = new();
            cancellationTokenSource.CancelAfter(timeout);
            var cancellationToken = cancellationTokenSource.Token;
            while (
                !cancellationToken.IsCancellationRequested
            )
            {
                try
                {
                    var consumeResult = Consumer!.Consume(cancellationToken);

                    if (consumeResult.IsPartitionEOF)
                    {
                        continue;
                    }

                    return false;
                }
                catch (OperationCanceledException)
                {
                }
            }
            return true;
        }
    }
}
