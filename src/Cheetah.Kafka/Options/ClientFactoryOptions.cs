using System;
using Cheetah.Kafka.Serdes;
using Confluent.Kafka;

namespace Cheetah.Kafka
{
    /// <summary>
    /// Options for configuring the <see cref="KafkaClientFactory"/>
    /// </summary>
    public class ClientFactoryOptions
    {
        private Action<ProducerConfig> _defaultProducerConfigure = config => { };
        private Action<ConsumerConfig> _defaultConsumerConfigure = config => { };
        private Action<AdminClientConfig> _defaultAdminClientConfigure = config => { };
        internal Action<ClientConfig> ClientConfigure { get; private set; } = config => { };

        internal Func<IServiceProvider, ISerializerProvider> SerializerProviderFactory = Utf8SerializerProvider.FromServices();
        internal Func<IServiceProvider, IDeserializerProvider> DeserializerProviderFactory = Utf8DeserializerProvider.FromServices();

        // This structure allows us to easily access the combined configuration for each client type
        internal Action<ProducerConfig> ProducerConfigure => MergeActions(ClientConfigure, _defaultProducerConfigure);
        internal Action<ConsumerConfig> ConsumerConfigure => MergeActions(ClientConfigure, _defaultConsumerConfigure);
        internal Action<AdminClientConfig> AdminClientConfigure => MergeActions(ClientConfigure, _defaultAdminClientConfigure);

        /// <summary>
        /// Configures the default <see cref="ClientConfig"/> that will be used for all clients created by the factory
        /// </summary>
        /// <param name="configure">The configuration to apply</param>
        /// <returns>This <see cref="ClientFactoryOptions"/> instance for method chaining</returns>
        public ClientFactoryOptions ConfigureDefaultClient(Action<ClientConfig> configure)
        {
            ClientConfigure = configure;
            return this;
        }

        /// <summary>
        /// Configures the default <see cref="ProducerConfig"/> that will be used for all producers created by the factory
        /// </summary>
        /// <remarks>This is applied <b>after</b> the default client configuration</remarks>
        /// <param name="configure">The configuration to apply</param>
        /// <returns>This <see cref="ClientFactoryOptions"/> instance for method chaining</returns>
        public ClientFactoryOptions ConfigureDefaultProducer(Action<ProducerConfig> configure)
        {
            _defaultProducerConfigure = configure;
            return this;
        }

        /// <summary>
        /// Configures the default <see cref="ConsumerConfig"/> that will be used for all consumers created by the factory
        /// </summary>
        /// <remarks>This is applied <b>after</b> the default client configuration</remarks>
        /// <param name="configure">The configuration to apply</param>
        /// <returns>This <see cref="ClientFactoryOptions"/> instance for method chaining</returns>
        public ClientFactoryOptions ConfigureDefaultConsumer(Action<ConsumerConfig> configure)
        {
            _defaultConsumerConfigure = configure;
            return this;
        }

        /// <summary>
        /// Configures the default <see cref="AdminClientConfig"/> that will be used for all admin clients created by the factory
        /// </summary>
        /// <remarks>This is applied <b>after</b> the default client configuration</remarks>
        /// <param name="configure">The configuration to apply</param>
        /// <returns>This <see cref="ClientFactoryOptions"/> instance for method chaining</returns>
        public ClientFactoryOptions ConfigureDefaultAdminClient(Action<AdminClientConfig> configure)
        {
            _defaultAdminClientConfigure = configure;
            return this;
        }

        /// <summary>
        /// Configures the default SerializerProviderFactory that will be used for all clients created by the factory
        /// </summary>
        /// <param name="serializerProviderFactory">The factory method for creating the default serializer provider.</param>
        /// <returns>This <see cref="ClientFactoryOptions"/> instance for method chaining</returns>
        public ClientFactoryOptions ConfigureDefaultSerializerProvider(Func<IServiceProvider, ISerializerProvider> serializerProviderFactory)
        {
            SerializerProviderFactory = serializerProviderFactory;
            return this;
        }

        /// <summary>
        /// Configures the default SerializerProvider that will be used for all clients created by the factory
        /// </summary>
        /// <param name="serializerProvider">The serializer provider to be used as default.</param>
        /// <returns>This <see cref="ClientFactoryOptions"/> instance for method chaining</returns>
        public ClientFactoryOptions ConfigureDefaultSerializerProvider(ISerializerProvider serializerProvider)
        {
            SerializerProviderFactory = _ => serializerProvider;
            return this;
        }
        /// <summary>
        /// Configures the default DeserializerProviderFactory that will be used for all clients created by the factory
        /// </summary>
        /// <param name="deserializerProviderFactory">The factory method for creating the default deserializer provider.</param>
        /// <returns>This <see cref="ClientFactoryOptions"/> instance for method chaining</returns>
        public ClientFactoryOptions ConfigureDefaultDeserializerProvider(Func<IServiceProvider, IDeserializerProvider> deserializerProviderFactory)
        {
            DeserializerProviderFactory = deserializerProviderFactory;
            return this;
        }

        /// <summary>
        /// Configures the default DeserializerProvider that will be used for all clients created by the factory
        /// </summary>
        /// <param name="deserializerProvider">The deserializer provider to be used as default.</param>
        /// <returns>This <see cref="ClientFactoryOptions"/> instance for method chaining</returns>
        public ClientFactoryOptions ConfigureDefaultDeserializerProvider(IDeserializerProvider deserializerProvider)
        {
            DeserializerProviderFactory = _ => deserializerProvider;
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
