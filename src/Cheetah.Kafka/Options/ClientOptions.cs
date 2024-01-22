using System;
using Confluent.Kafka;

namespace Cheetah.Kafka;

public class ClientOptions<TConfig, TBuilder> where  TConfig : ClientConfig where TBuilder : class
{
    internal Action<TConfig>? ConfigureAction { get; private set; }
    internal Action<TBuilder>? BuilderAction { get; private set; }

    public void ConfigureClient(Action<TConfig> configureAction)
    {
        ConfigureAction = configureAction;
    }
        
    public void ConfigureBuilder(Action<TBuilder> builderAction)
    {
        BuilderAction = builderAction;
    }
}
