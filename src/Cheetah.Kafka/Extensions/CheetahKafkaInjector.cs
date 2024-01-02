using System;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Kafka.Extensions
{
    /// <summary>
    /// Utility class used to inject Kafka clients into a service collection
    /// </summary>
    public class CheetahKafkaInjector
    {
        private readonly IServiceCollection _serviceCollection;

        internal CheetahKafkaInjector(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }
        
        /// <summary>
        /// Registers a pre-configured <see cref="IProducer{TKey,TValue}"/>/>
        /// </summary>
        /// <param name="configAction">Additional configuration that this specific producer should use</param>
        /// <typeparam name="TKey">The type of key that the injected producer will produce</typeparam>
        /// <typeparam name="TValue">The type of value that the injected producer will produce</typeparam>
        /// <returns>This <see cref="CheetahKafkaInjector"/> instance for method chaining</returns>
        public CheetahKafkaInjector WithProducer<TKey, TValue>(Action<ProducerConfig>? configAction = null)
        {
            _serviceCollection.AddSingleton(provider => GetFactory(provider).CreateProducer<TKey, TValue>(configAction));
            return this;
        }
        
        /// <summary>
        /// Registers a pre-configured, keyed <see cref="IProducer{TKey,TValue}"/>/>
        /// </summary>
        /// <param name="key">The key that the producer should be registered with</param>
        /// <param name="configAction">Additional configuration that this specific producer should use</param>
        /// <typeparam name="TKey">The type of key that the injected producer will produce</typeparam>
        /// <typeparam name="TValue">The type of value that the injected producer will produce</typeparam>
        /// <returns>This <see cref="CheetahKafkaInjector"/> instance for method chaining</returns>
        public CheetahKafkaInjector WithKeyedProducer<TKey, TValue>(object? key, Action<ProducerConfig>? configAction = null)
        {
            _serviceCollection.AddKeyedSingleton(key, (provider, o) => GetFactory(provider).CreateProducer<TKey, TValue>(configAction) );
            return this;
        }
        
        /// <summary>
        /// Registers a pre-configured <see cref="IConsumer{TKey,TValue}"/>/>
        /// </summary>
        /// <param name="configAction">Additional configuration that this specific consumer should use</param>
        /// <typeparam name="TKey">The type of key that the injected consumer will consume</typeparam>
        /// <typeparam name="TValue">The type of value that the injected consumer will consume</typeparam>
        /// <returns>This <see cref="CheetahKafkaInjector"/> instance for method chaining</returns>
        public CheetahKafkaInjector WithConsumer<TKey, TValue>(Action<ConsumerConfig>? configAction = null)
        {
            _serviceCollection.AddSingleton(provider => GetFactory(provider).CreateConsumer<TKey, TValue>(configAction));
            return this;
        }

        /// <summary>
        /// Registers a pre-configured, keyed <see cref="IConsumer{TKey,TValue}"/>/>
        /// </summary>
        /// <param name="configAction">Additional configuration that this specific consumer should use</param>
        /// <param name="key">The key that the consumer should be registered with</param>
        /// <typeparam name="TKey">The type of key that the injected consumer will consume</typeparam>
        /// <typeparam name="TValue">The type of value that the injected consumer will consume</typeparam>
        /// <returns>This <see cref="CheetahKafkaInjector"/> instance for method chaining</returns>
        public CheetahKafkaInjector WithKeyedConsumer<TKey, TValue>(object? key, Action<ConsumerConfig>? configAction = null)
        {
            _serviceCollection.AddKeyedSingleton(key, (provider, o) => GetFactory(provider).CreateConsumer<TKey, TValue>(configAction));
            return this;
        }

        /// <summary>
        /// Registers a pre-configured <see cref="IAdminClient"/>/>
        /// </summary>
        /// <param name="configAction">Additional configuration that this specific admin client should use</param>
        /// <returns></returns>
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

