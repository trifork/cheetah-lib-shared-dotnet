using System;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Kafka.Extensions
{
    public class CheetahKafkaInjector
    {
        private readonly IServiceCollection _serviceCollection;

        internal CheetahKafkaInjector(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }
        public CheetahKafkaInjector WithProducer<TKey, TValue>(Action<ProducerConfig>? configAction = null)
        {
            _serviceCollection.AddSingleton(provider => GetFactory(provider).CreateProducer<TKey, TValue>(configAction));
            return this;
        }
        
        public CheetahKafkaInjector WithKeyedProducer<TKey, TValue>(object? key, Action<ProducerConfig>? configAction = null)
        {
            _serviceCollection.AddKeyedSingleton(key, (provider, o) => GetFactory(provider).CreateProducer<TKey, TValue>(configAction) );
            return this;
        }
        public CheetahKafkaInjector WithConsumer<TKey, TValue>(Action<ConsumerConfig>? configAction = null)
        {
            _serviceCollection.AddSingleton(provider => GetFactory(provider).CreateConsumer<TKey, TValue>(configAction));
            return this;
        }

        public CheetahKafkaInjector WithKeyedConsumer<TKey, TValue>(object? key, Action<ConsumerConfig>? configAction = null)
        {
            _serviceCollection.AddKeyedSingleton(key, (provider, o) => GetFactory(provider).CreateConsumer<TKey, TValue>(configAction));
            return this;
        }

        public CheetahKafkaInjector WithAdminClient(Action<AdminClientConfig>? configAction = null)
        {
            _serviceCollection.AddSingleton(provider => GetFactory(provider).CreateAdminClient(configAction));
            return this;
        }
        
        public CheetahKafkaInjector WithKeyedAdminClient(object? key, Action<AdminClientConfig>? configAction = null)
        {
            _serviceCollection.AddKeyedSingleton(key, (provider, o) => GetFactory(provider).CreateAdminClient(configAction));
            return this;
        }

        private static IKafkaClientFactory GetFactory(IServiceProvider provider) => provider.GetRequiredService<IKafkaClientFactory>();
    }
}

