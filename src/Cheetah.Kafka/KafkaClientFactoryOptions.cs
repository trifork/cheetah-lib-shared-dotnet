using System;
using Confluent.Kafka;

namespace Cheetah.Kafka
{
    /// <summary>
    /// Options for configuring the <see cref="KafkaClientFactory"/>
    /// </summary>
    public class KafkaClientFactoryOptions
    {
        private Action<ProducerConfig> _defaultProducerConfigure = config => { };
        private Action<ConsumerConfig> _defaultConsumerConfigure = config => { };
        private Action<AdminClientConfig> _defaultAdminClientConfigure = config => { };

        internal Action<ClientConfig> DefaultClientConfigure { get; private set; } = config => { };

        // This structure allows us to easily access the combined configuration for each client type
        internal Action<ProducerConfig> DefaultProducerConfigure =>
            MergeActions(DefaultClientConfigure, _defaultProducerConfigure);
        internal Action<ConsumerConfig> DefaultConsumerConfigure =>
            MergeActions(DefaultClientConfigure, _defaultConsumerConfigure);
        internal Action<AdminClientConfig> DefaultAdminClientConfigure =>
            MergeActions(DefaultClientConfigure, _defaultAdminClientConfigure);

        /// <summary>
        /// Configures the default <see cref="ClientConfig"/> that will be used for all clients created by the factory
        /// </summary>
        /// <param name="configure">The configuration to apply</param>
        /// <returns>This <see cref="KafkaClientFactoryOptions"/> instance for method chaining</returns>
        public KafkaClientFactoryOptions ConfigureDefaultClient(Action<ClientConfig> configure)
        {
            DefaultClientConfigure = configure;
            return this;
        }

        /// <summary>
        /// Configures the default <see cref="ProducerConfig"/> that will be used for all producers created by the factory
        /// </summary>
        /// <remarks>This is applied <b>after</b> the default client configuration</remarks>
        /// <param name="configure">The configuration to apply</param>
        /// <returns>This <see cref="KafkaClientFactoryOptions"/> instance for method chaining</returns>
        public KafkaClientFactoryOptions ConfigureDefaultProducer(Action<ProducerConfig> configure)
        {
            _defaultProducerConfigure = configure;
            return this;
        }

        /// <summary>
        /// Configures the default <see cref="ConsumerConfig"/> that will be used for all consumers created by the factory
        /// </summary>
        /// <remarks>This is applied <b>after</b> the default client configuration</remarks>
        /// <param name="configure">The configuration to apply</param>
        /// <returns>This <see cref="KafkaClientFactoryOptions"/> instance for method chaining</returns>
        public KafkaClientFactoryOptions ConfigureDefaultConsumer(Action<ConsumerConfig> configure)
        {
            _defaultConsumerConfigure = configure;
            return this;
        }

        /// <summary>
        /// Configures the default <see cref="AdminClientConfig"/> that will be used for all admin clients created by the factory
        /// </summary>
        /// <remarks>This is applied <b>after</b> the default client configuration</remarks>
        /// <param name="configure">The configuration to apply</param>
        /// <returns>This <see cref="KafkaClientFactoryOptions"/> instance for method chaining</returns>
        public KafkaClientFactoryOptions ConfigureDefaultAdminClient(
            Action<AdminClientConfig> configure
        )
        {
            _defaultAdminClientConfigure = configure;
            return this;
        }

        /// <summary>
        /// Merges multiple <see cref="Action{T}"/> into a single <see cref="Action{T}"/>
        /// </summary>
        /// <param name="actions">The actions to merge</param>
        /// <typeparam name="T">The type that the action should operate on</typeparam>
        /// <returns>The merged action</returns>
        private static Action<T> MergeActions<T>(params Action<T>[] actions)
        {
            return cfg =>
            {
                foreach (var action in actions)
                {
                    action(cfg);
                }
            };
        }
    }
}
