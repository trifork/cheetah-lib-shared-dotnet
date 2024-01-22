using System;
using Confluent.Kafka;

namespace Cheetah.Kafka;

public class ConsumerOptions<TKey, TValue> : ClientOptions<ConsumerConfig, ConsumerBuilder<TKey, TValue>>
{
    internal IDeserializer<TValue>? Deserializer { get; private set; }
        
    public void SetDeserializer(IDeserializer<TValue> deserializer)
    {
        Deserializer = deserializer;
    }
    
}

internal interface IOptionsBuilder<out TOptions> where TOptions : new()
{
    TOptions Build(IServiceProvider serviceProvider);
}

public class ConsumerOptionsBuilder<TKey, TValue> : IOptionsBuilder<ConsumerOptions<TKey, TValue>>
{
    private readonly ConsumerOptions<TKey, TValue> _options = new();
    private Func<IServiceProvider, IDeserializer<TValue>>? _deserializerFactory;

    public ConsumerOptionsBuilder<TKey, TValue> SetDeserializer(Func<IServiceProvider, IDeserializer<TValue>> deserializerFactory)
    {
        _deserializerFactory = deserializerFactory;
        return this;
    }
    
    public ConsumerOptionsBuilder<TKey, TValue> ConfigureClient(Action<ConsumerConfig> configureAction)
    {
        _options.ConfigureClient(configureAction);
        return this;
    }
    
    public ConsumerOptionsBuilder<TKey, TValue> ConfigureBuilder(Action<ConsumerBuilder<TKey, TValue>> builderAction)
    {
        _options.ConfigureBuilder(builderAction);
        return this;
    }
    
    public ConsumerOptions<TKey, TValue> Build(IServiceProvider serviceProvider)
    {
        if (_deserializerFactory != null)
        {
            _options.SetDeserializer(_deserializerFactory.Invoke(serviceProvider));
        }
        
        return _options;
    }
}
