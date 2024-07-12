using System;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Kafka.Extensions
{
    /// <summary>
    /// Utility class used to inject Kafka clients into a service collection
    /// </summary>
    public class ClientInjector
    {
        private readonly IServiceCollection _serviceCollection;

        /// <summary>
        /// Creates a new instance of <see cref="ClientInjector"/>
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to add the services to.</param>
        public ClientInjector(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        /// <summary>
        /// Registers a pre-configured <see cref="IProducer{TKey,TValue}"/>/>
        /// </summary>
        /// <param name="configAction">Additional configuration that this specific producer should use</param>
        /// <typeparam name="TKey">The type of key that the injected producer will produce</typeparam>
        /// <typeparam name="TValue">The type of value that the injected producer will produce</typeparam>
        /// <returns>This <see cref="ClientInjector"/> instance for method chaining</returns>
        public ClientInjector WithProducer<TKey, TValue>(Action<ProducerOptionsBuilder<TKey, TValue>>? configAction = null)
        {
            _serviceCollection.AddSingleton(provider => CreateProducer(provider, configAction));
            return this;
        }

        /// <summary>
        /// Registers a pre-configured, keyed <see cref="IProducer{TKey,TValue}"/>/>
        /// </summary>
        /// <param name="key">The key that the producer should be registered with</param>
        /// <param name="configAction">Additional configuration that this specific producer should use</param>
        /// <typeparam name="TKey">The type of key that the injected producer will produce</typeparam>
        /// <typeparam name="TValue">The type of value that the injected producer will produce</typeparam>
        /// <returns>This <see cref="ClientInjector"/> instance for method chaining</returns>
        public ClientInjector WithKeyedProducer<TKey, TValue>(object? key, Action<ProducerOptionsBuilder<TKey, TValue>>? configAction = null)
        {
            _serviceCollection.AddKeyedSingleton(key, (provider, o) => CreateProducer(provider, configAction));
            return this;
        }

        /// <summary>
        /// Registers a pre-configured <see cref="IConsumer{TKey,TValue}"/>/>
        /// </summary>
        /// <param name="configAction">Additional configuration that this specific consumer should use</param>
        /// <typeparam name="TKey">The type of key that the injected consumer will consume</typeparam>
        /// <typeparam name="TValue">The type of value that the injected consumer will consume</typeparam>
        /// <returns>This <see cref="ClientInjector"/> instance for method chaining</returns>
        public ClientInjector WithConsumer<TKey, TValue>(Action<ConsumerOptionsBuilder<TKey, TValue>>? configAction = null)
        {
            _serviceCollection.AddSingleton(provider => CreateConsumer(provider, configAction));
            return this;
        }

        /// <summary>
        /// Registers a pre-configured, keyed <see cref="IConsumer{TKey,TValue}"/>/>
        /// </summary>
        /// <param name="key">The key that the consumer should be registered with</param>
        /// <param name="configAction">Additional configuration that this specific consumer should use</param>
        /// <typeparam name="TKey">The type of key that the injected consumer will consume</typeparam>
        /// <typeparam name="TValue">The type of value that the injected consumer will consume</typeparam>
        /// <returns>This <see cref="ClientInjector"/> instance for method chaining</returns>
        public ClientInjector WithKeyedConsumer<TKey, TValue>(object? key, Action<ConsumerOptionsBuilder<TKey, TValue>>? configAction = null)
        {
            _serviceCollection.AddKeyedSingleton(key, (provider, o) => CreateConsumer(provider, configAction));
            return this;
        }

        /// <summary>
        /// Registers a pre-configured <see cref="IAdminClient"/>/>
        /// </summary>
        /// <param name="configAction">Additional configuration that this specific admin client should use</param>
        /// <returns>This <see cref="ClientInjector"/> instance for method chaining</returns>
        public ClientInjector WithAdminClient(Action<AdminClientOptionsBuilder>? configAction = null)
        {
            _serviceCollection.AddSingleton(provider => CreateAdminClient(provider, configAction));
            return this;
        }

        /// <summary>
        /// Registers a pre-configured <see cref="IAdminClient"/>/>
        /// </summary>
        /// <param name="key">The key that the admin client should be registered with</param>
        /// <param name="configAction">Additional configuration that this specific admin client should use</param>>
        /// <returns>This <see cref="ClientInjector"/> instance for method chaining</returns>
        public ClientInjector WithKeyedAdminClient(object? key, Action<AdminClientOptionsBuilder>? configAction = null)
        {
            _serviceCollection.AddKeyedSingleton(key, (provider, o) => CreateAdminClient(provider, configAction));
            return this;
        }

        private static IProducer<TKey, TValue> CreateProducer<TKey, TValue>(IServiceProvider serviceProvider, Action<ProducerOptionsBuilder<TKey, TValue>>? configAction = null)
        {
            var options = BuildOptions<ProducerOptionsBuilder<TKey, TValue>, ProducerOptions<TKey, TValue>>(serviceProvider, configAction);
            return GetFactory(serviceProvider).CreateProducer(options);
        }

        private static IConsumer<TKey, TValue> CreateConsumer<TKey, TValue>(IServiceProvider serviceProvider, Action<ConsumerOptionsBuilder<TKey, TValue>>? configAction = null)
        {
            var options = BuildOptions<ConsumerOptionsBuilder<TKey, TValue>, ConsumerOptions<TKey, TValue>>(serviceProvider, configAction);
            return GetFactory(serviceProvider).CreateConsumer(options);
        }

        private static IAdminClient CreateAdminClient(IServiceProvider serviceProvider, Action<AdminClientOptionsBuilder>? configAction = null)
        {
            var options = BuildOptions<AdminClientOptionsBuilder, AdminClientOptions>(serviceProvider, configAction);
            return GetFactory(serviceProvider).CreateAdminClient(options);
        }

        private static TOptions BuildOptions<TOptionsBuilder, TOptions>(IServiceProvider provider, params Action<TOptionsBuilder>?[] configureActions)
            where TOptionsBuilder : IOptionsBuilder<TOptions>, new()
            where TOptions : new()
        {
            var optionsBuilder = new TOptionsBuilder();

            foreach (var buildAction in configureActions)
            {
                buildAction?.Invoke(optionsBuilder);
            }

            return optionsBuilder.Build(provider);
        }

        private static KafkaClientFactory GetFactory(IServiceProvider provider)
        {
            return provider.GetRequiredService<KafkaClientFactory>();
        }
    }
}

