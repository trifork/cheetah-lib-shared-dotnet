using System;
using Confluent.Kafka;

namespace Cheetah.Kafka
{
    /// <summary>
    /// Represents options for configuring a Kafka producer.
    /// </summary>
    /// <typeparam name="TKey">The type of the producer key.</typeparam>
    /// <typeparam name="TValue">The type of the producer value.</typeparam>
    public class ProducerOptions<TKey, TValue> : ClientOptions<ProducerConfig, ProducerBuilder<TKey, TValue>>
    {
        internal ISerializer<TKey>? KeySerializer { get; private set; }
        internal ISerializer<TValue>? ValueSerializer { get; private set; }

        /// <summary>
        /// Sets the value serializer for the producer.
        /// </summary>
        /// <param name="valueSerializer">The serializer to be used for the producer.</param>
        public void SetValueSerializer(ISerializer<TValue> valueSerializer)
        {
            ValueSerializer = valueSerializer;
        }

        /// <summary>
        /// Sets the key serializer for the producer.
        /// </summary>
        /// <param name="keySerializer">The serializer to be used for the producer.</param>
        public void SetKeySerializer(ISerializer<TKey> keySerializer)
        {
            KeySerializer = keySerializer;
        }
    }

    /// <summary>
    /// Builder for configuring <see cref="ProducerOptions{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the producer key.</typeparam>
    /// <typeparam name="TValue">The type of the producer value.</typeparam>
    public class ProducerOptionsBuilder<TKey, TValue> : IOptionsBuilder<ProducerOptions<TKey, TValue>>
    {
        private readonly ProducerOptions<TKey, TValue> _options = new ProducerOptions<TKey, TValue>();
        private Func<IServiceProvider, ISerializer<TValue>>? _valueSerializerFactory;
        private Func<IServiceProvider, ISerializer<TKey>>? _keySerializerFactory;

        /// <summary>
        /// Sets the value serializer factory method.
        /// </summary>
        /// <param name="valueSerializerFactory">The factory method for creating the value serializer.</param>
        /// <returns>The builder instance.</returns>
        public ProducerOptionsBuilder<TKey, TValue> SetValueSerializer(Func<IServiceProvider, ISerializer<TValue>> valueSerializerFactory)
        {
            _valueSerializerFactory = valueSerializerFactory;
            return this;
        }

        /// <summary>
        /// Sets the value serializer.
        /// </summary>
        /// <param name="valueSerializer">The value serializer.</param>
        /// <returns>The builder instance.</returns>
        public ProducerOptionsBuilder<TKey, TValue> SetValueSerializer(ISerializer<TValue> valueSerializer)
        {
            _options.SetValueSerializer(valueSerializer);
            return this;
        }

        /// <summary>
        /// Sets the key serializer factory method.
        /// </summary>
        /// <param name="keySerializerFactory">The factory method for creating the key serializer.</param>
        /// <returns>The builder instance.</returns>
        public ProducerOptionsBuilder<TKey, TValue> SetKeySerializer(Func<IServiceProvider, ISerializer<TKey>> keySerializerFactory)
        {
            _keySerializerFactory = keySerializerFactory;
            return this;
        }

        /// <summary>
        /// Sets the key serializer.
        /// </summary>
        /// <param name="keySerializer">The key serializer.</param>
        /// <returns>The builder instance.</returns>
        public ProducerOptionsBuilder<TKey, TValue> SetKeySerializer(ISerializer<TKey> keySerializer)
        {
            _options.SetKeySerializer(keySerializer);
            return this;
        }

        /// <summary>
        /// Configures the producer with the provided action.
        /// </summary>
        /// <param name="configureAction">The action to configure the producer.</param>
        /// <returns>The builder instance.</returns>
        public ProducerOptionsBuilder<TKey, TValue> ConfigureClient(Action<ProducerConfig> configureAction)
        {
            _options.ConfigureClient(configureAction);
            return this;
        }

        /// <summary>
        /// Configures the producer builder with the provided action.
        /// </summary>
        /// <param name="builderAction">The action to configure the producer builder.</param>
        /// <returns>The builder instance.</returns>
        public ProducerOptionsBuilder<TKey, TValue> ConfigureBuilder(Action<ProducerBuilder<TKey, TValue>> builderAction)
        {
            _options.ConfigureBuilder(builderAction);
            return this;
        }

        // TODO: This should probably be internal, but we can't implement an interface with internal methods.
        // TODO: Has to be replaced with abstract base + specializations if we want to do that.
        /// <summary>
        /// Builds the producer options.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The configured producer options.</returns>
        public ProducerOptions<TKey, TValue> Build(IServiceProvider serviceProvider)
        {
            if (_valueSerializerFactory != null)
            {
                _options.SetValueSerializer(_valueSerializerFactory.Invoke(serviceProvider));
            }
            if (_keySerializerFactory != null)
            {
                _options.SetKeySerializer(_keySerializerFactory.Invoke(serviceProvider));
            }

            return _options;
        }

        /// <summary>
        /// Builds the producer options.
        /// </summary>
        /// <returns>The configured producer options.</returns>
        public ProducerOptions<TKey, TValue> Build()
        {
            return _options;
        }

    }
}
