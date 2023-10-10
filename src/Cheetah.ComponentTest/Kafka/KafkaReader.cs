using System;
using System.Collections.Generic;
using System.Threading;
using Cheetah.Core.Infrastructure.Services.Kafka;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Cheetah.ComponentTest.Kafka
{
    public class KafkaReader<TKey, T>
    {
        private static readonly ILogger Logger = new LoggerFactory().CreateLogger<KafkaReader<TKey, T>>();
        internal string Topic { get; }
        private IConsumer<TKey, T> Consumer { get; }

        internal KafkaReader(KafkaReaderProps<T> props)
        {
            Topic = props.Topic;
            Logger.LogInformation("Preparing kafka producer, producing to topic '{Topic}'", Topic);
            Consumer = new ConsumerBuilder<TKey, T>(new ConsumerConfig
            {
                BootstrapServers = props.KafkaUrl,
                GroupId = props.ConsumerGroup,
                SaslMechanism = SaslMechanism.OAuthBearer,
                SecurityProtocol = SecurityProtocol.SaslPlaintext,
                EnablePartitionEof = true,
                AllowAutoCreateTopics = true,
                AutoOffsetReset = AutoOffsetReset.Latest
            })
                .SetValueDeserializer(props.Deserializer)
                .AddCheetahOAuthentication(props.TokenService, Logger)
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

        /// <summary>
        /// Reads up to <paramref name="count"/> messages from the configured Kafka topic.
        /// </summary>
        /// <remarks>
        /// Usually this method will require a rather large timeout, especially if it is the first read in a test.
        /// This recommendation is based on the fact that the tested job usually needs some time to start up.
        /// </remarks>
        /// <param name="count">The amount of messages to write</param>
        /// <param name="timeout">The maximum time to wait for the required number of messages to be available</param>
        /// <returns>The read messages</returns>
        /// <exception cref="InvalidOperationException">Thrown when the reader could not read enough messages within the allotted time</exception>
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
            while (messages.Count < count && !cancellationToken.IsCancellationRequested)
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

        /// <summary>
        /// Tests that there are no more available messages.
        /// </summary>
        /// <remarks>
        /// This method is usually useful in conjunction with the <see cref="ReadMessages"/> method in order to ensure,
        /// that we read not just the expected amount of messages, but also verify that there are no other, unexpected messages.
        /// </remarks>
        /// <param name="timeout">The amount of time to listen for unexpected messages.</param>
        /// <returns><c>true</c> if no other messages were found after the timeout, otherwise <c>false</c></returns>
        public bool VerifyNoMoreMessages(TimeSpan timeout)
        {
            CancellationTokenSource cancellationTokenSource = new();
            cancellationTokenSource.CancelAfter(timeout);
            var cancellationToken = cancellationTokenSource.Token;
            while (!cancellationToken.IsCancellationRequested)
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
