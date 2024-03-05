using System;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace Cheetah.Kafka
{
    public class ProducerOptions<TKey, TValue> : ClientOptions<ProducerConfig, ProducerBuilder<TKey, TValue>>
    {
        // TODO: Add Key Serializer
        internal ISerializer<TValue>? Serializer { get; private set; }
        
        public void SetSerializer(ISerializer<TValue> serializer)
        {
            Serializer = serializer;
        }
    }

    public class ProducerOptionsBuilder<TKey, TValue> : IOptionsBuilder<ProducerOptions<TKey, TValue>>
    {
        private readonly ProducerOptions<TKey, TValue> _options = new ProducerOptions<TKey, TValue>();
        private Func<IServiceProvider, ISerializer<TValue>>? _serializerFactory;

        public ProducerOptionsBuilder<TKey, TValue> SetSerializer(Func<IServiceProvider, ISerializer<TValue>> serializerFactory)
        {
            _serializerFactory = serializerFactory;
            return this;
        }
    
        public ProducerOptionsBuilder<TKey, TValue> ConfigureClient(Action<ProducerConfig> configureAction)
        {
            _options.ConfigureClient(configureAction);
            return this;
        }
    
        public ProducerOptionsBuilder<TKey, TValue> ConfigureBuilder(Action<ProducerBuilder<TKey, TValue>> builderAction)
        {
            _options.ConfigureBuilder(builderAction);
            return this;
        }
    
        // TODO: This should probably be internal, but we can't implement an interface with internal methods.
        // TODO: Has to be replaced with abstract base + specializations if we want to do that.
        public ProducerOptions<TKey, TValue> Build(IServiceProvider serviceProvider)
        {
            if (_serializerFactory != null)
            {
                _options.SetSerializer(_serializerFactory.Invoke(serviceProvider));
            }
        
            return _options;
        }
        public ProducerOptions<TKey, TValue> Build()
        {
            return _options;
        }
        
    }
}
