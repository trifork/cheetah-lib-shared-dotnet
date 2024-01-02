using System;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Cheetah.Kafka.Testing
{

    /// <inheritdoc cref="KafkaTestWriter{TKey, T}"/>
    public interface IKafkaTestWriter<TKey, T>
    {
        /// <inheritdoc cref="KafkaTestWriter{TKey, T}.WriteAsync"/>
        public Task<DeliveryResult<TKey, T>[]> WriteAsync(params T[] messages);
    }
    
    /// <summary>
    /// A simple Kafka client used to write messages to a Kafka topic.
    /// </summary>
    /// <remarks>
    /// This should only be used for testing purposes, and has no performance guarantees.
    /// </remarks>
    /// <typeparam name="TKey">The type of key to produce</typeparam>
    /// <typeparam name="T">The type of value to produce</typeparam>
    public class KafkaTestWriter<TKey, T> : IKafkaTestWriter<TKey, T>
    {
        private string Topic { get; }
        private Func<T, TKey> KeyFunction { get; }
        private IProducer<TKey, T> Producer { get; }

        internal KafkaTestWriter(IProducer<TKey, T> producer, Func<T, TKey> keyFunction, string topic)
        {
            Topic = topic;
            KeyFunction = keyFunction;
            Producer = producer;
        }

        /// <summary>
        /// Publishes multiple messages to Kafka
        /// </summary>
        /// <param name="messages">The collection of messages to publish</param>
        /// <exception cref="ArgumentException">Thrown if the provided collection of messages is empty</exception>
        public Task<DeliveryResult<TKey, T>[]> WriteAsync(params T[] messages)
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
