using System;
using System.Collections.Generic;
using System.Threading;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Cheetah.Kafka.Testing
{
    /// <inheritdoc cref="KafkaTestReader{TKey,T}"/>
    public interface IKafkaTestReader<TKey, T>
    {
        /// <inheritdoc cref="KafkaTestReader{TKey,T}.ReadMessages"/>
        public IEnumerable<Message<TKey, T>> ReadMessages(int count, TimeSpan timeout);

        /// <inheritdoc cref="KafkaTestReader{TKey,T}.VerifyNoMoreMessages"/>
        public bool VerifyNoMoreMessages(TimeSpan timeout);
    }

    /// <summary>
    /// A simple Kafka client used to read messages from a Kafka topic.
    /// </summary>
    /// <remarks>
    /// This should only be used for testing purposes, and has no performance guarantees.
    /// </remarks>
    /// <typeparam name="TKey">The type of key that read messages have</typeparam>
    /// <typeparam name="T">The type of message to read</typeparam>
    public class KafkaTestReader<TKey, T> : IKafkaTestReader<TKey, T>
    {
        private static readonly ILogger Logger = new LoggerFactory().CreateLogger<
            KafkaTestReader<TKey, T>
        >();
        private string Topic { get; }
        private IConsumer<TKey, T> Consumer { get; }

        internal KafkaTestReader(IConsumer<TKey, T> consumer, string topic)
        {
            Topic = topic;
            Logger.LogInformation("Preparing kafka producer, producing to topic '{Topic}'", Topic);
            Consumer = consumer;

            Consumer.Assign(new TopicPartition(Topic, 0));
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
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
        public IEnumerable<Message<TKey, T>> ReadMessages(int count, TimeSpan timeout)
        {
            var messages = new List<Message<TKey, T>>();

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(timeout);
            var cancellationToken = cancellationTokenSource.Token;
            Logger.LogInformation(
                "Consuming messages from '{topic}', expecting a total of {count} messages...",
                Topic,
                count
            );
            while (messages.Count < count && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = Consumer.Consume(cancellationToken);
                    if (consumeResult.IsPartitionEOF)
                    {
                        continue;
                    }

                    messages.Add(consumeResult.Message);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            if (messages.Count < count)
            {
                throw new InvalidOperationException(
                    $"Could not read enough messages from {Topic}, read {messages.Count}, expected {count}"
                );
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
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
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
                catch (OperationCanceledException) { }
            }
            return true;
        }
    }
}
