using System;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace Cheetah.Kafka
{
    /// <summary>
    /// Represents options for configuring a Kafka producer.
    /// </summary>
    /// <typeparam name="TKey">The type of the producer key.</typeparam>
    /// <typeparam name="TValue">The type of the producer value.</typeparam>
    public class ProducerOptions<TKey, TValue> : ClientOptions<ProducerConfig, ProducerBuilder<TKey, TValue>>
    {
        // TODO: Add Key Serializer
        internal ISerializer<TValue>? Serializer { get; private set; }
        
        /// <summary>
        /// Sets the serializer for the producer.
        /// </summary>
        /// <param name="serializer">The serializer to be used for the producer.</param>
        public void SetSerializer(ISerializer<TValue> serializer)
        {
            Serializer = serializer;
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
        private Func<IServiceProvider, ISerializer<TValue>>? _serializerFactory;

        /// <summary>
        /// Sets the serializer factory method.
        /// </summary>
        /// <param name="serializerFactory">The factory method for creating the serializer.</param>
        /// <returns>The builder instance.</returns>
        public ProducerOptionsBuilder<TKey, TValue> SetSerializer(Func<IServiceProvider, ISerializer<TValue>> serializerFactory)
        {
            _serializerFactory = serializerFactory;
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
            if (_serializerFactory != null)
            {
                _options.SetSerializer(_serializerFactory.Invoke(serviceProvider));
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
