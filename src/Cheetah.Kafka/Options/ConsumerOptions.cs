using System;
using Confluent.Kafka;

namespace Cheetah.Kafka
{
    /// <summary>
    /// Represents options for configuring a Kafka consumer.
    /// </summary>
    /// <typeparam name="TKey">The type of the consumer key.</typeparam>
    /// <typeparam name="TValue">The type of the consumer value.</typeparam>
    public class ConsumerOptions<TKey, TValue> : ClientOptions<ConsumerConfig, ConsumerBuilder<TKey, TValue>>
    {
        internal IDeserializer<TValue>? ValueDeserializer { get; private set; }
        internal IDeserializer<TKey>? KeyDeserializer { get; private set; }

        /// <summary>
        /// Sets the value deserializer for the consumer.
        /// </summary>
        /// <param name="valueDeserializer">The value deserializer to be used for the consumer.</param>
        public void SetValueDeserializer(IDeserializer<TValue> valueDeserializer)
        {
            ValueDeserializer = valueDeserializer;
        }

        /// <summary>
        /// Sets the key deserializer for the consumer.
        /// </summary>
        /// <param name="keyDeserializer">The key deserializer to be used for the consumer.</param>
        public void SetKeyDeserializer(IDeserializer<TKey> keyDeserializer)
        {
            KeyDeserializer = keyDeserializer;
        }

    }

    /// <summary>
    /// Builder for configuring <see cref="ConsumerOptions{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the consumer key.</typeparam>
    /// <typeparam name="TValue">The type of the consumer value.</typeparam>
    public class ConsumerOptionsBuilder<TKey, TValue> : IOptionsBuilder<ConsumerOptions<TKey, TValue>>
    {
        private readonly ConsumerOptions<TKey, TValue> _options = new ConsumerOptions<TKey, TValue>();
        private Func<IServiceProvider, IDeserializer<TValue>>? _valueDeserializerFactory;
        private Func<IServiceProvider, IDeserializer<TKey>>? _keyDeserializerFactory;

        /// <summary>
        /// Sets the value deserializer factory method.
        /// </summary>
        /// <param name="valueDeserializerFactory">The factory method for creating the value deserializer.</param>
        /// <returns>The builder instance.</returns>
        public ConsumerOptionsBuilder<TKey, TValue> SetValueDeserializer(Func<IServiceProvider, IDeserializer<TValue>> valueDeserializerFactory)
        {
            _valueDeserializerFactory = valueDeserializerFactory;
            return this;
        }

        /// <summary>
        /// Sets the key deserializer factory method.
        /// </summary>
        /// <param name="keyDeserializerFactory">The factory method for creating the key deserializer.</param>
        /// <returns>The builder instance.</returns>
        public ConsumerOptionsBuilder<TKey, TValue> SetKeyDeserializer(Func<IServiceProvider, IDeserializer<TKey>> keyDeserializerFactory)
        {
            _keyDeserializerFactory = keyDeserializerFactory;
            return this;
        }

        /// <summary>
        /// Configures the consumer with the provided action.
        /// </summary>
        /// <param name="configureAction">The action to configure the consumer.</param>
        /// <returns>The builder instance.</returns>
        public ConsumerOptionsBuilder<TKey, TValue> ConfigureClient(Action<ConsumerConfig> configureAction)
        {
            _options.ConfigureClient(configureAction);
            return this;
        }

        /// <summary>
        /// Configures the consumer builder with the provided action.
        /// </summary>
        /// <param name="builderAction">The action to configure the consumer builder.</param>
        /// <returns>The builder instance.</returns>
        public ConsumerOptionsBuilder<TKey, TValue> ConfigureBuilder(Action<ConsumerBuilder<TKey, TValue>> builderAction)
        {
            _options.ConfigureBuilder(builderAction);
            return this;
        }

        /// <summary>
        /// Builds the consumer options.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The configured consumer options.</returns>
        public ConsumerOptions<TKey, TValue> Build(IServiceProvider serviceProvider)
        {
            if (_valueDeserializerFactory != null)
            {
                _options.SetValueDeserializer(_valueDeserializerFactory.Invoke(serviceProvider));
            }
            if (_keyDeserializerFactory != null)
            {
                _options.SetKeyDeserializer(_keyDeserializerFactory.Invoke(serviceProvider));
            }

            return _options;
        }

        /// <summary>
        /// Builds the consumer options.
        /// </summary>
        /// <returns>The configured consumer options.</returns>
        public ConsumerOptions<TKey, TValue> Build()
        {
            return _options;
        }
    }
}
