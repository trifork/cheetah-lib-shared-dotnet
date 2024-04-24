using System;
using System.Threading;
using System.Threading.Tasks;
using Cheetah.Auth.Authentication;
using Cheetah.Kafka.Configuration;
using Cheetah.Kafka.Extensions;
using Cheetah.Kafka.Serialization;
using Cheetah.Kafka.Util;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Cheetah.Kafka
{
    /// <summary>
    /// Factory for creating Kafka clients
    /// </summary>
    public class KafkaClientFactory
    {
        private readonly ITokenService _kafkaTokenService;
        private readonly ILogger<KafkaClientFactory> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly KafkaConfig _config;
        private readonly ClientFactoryOptions _options;
        private readonly ISerializerProvider _serializerProvider;

        /// <summary>
        /// Creates a new instance of <see cref="KafkaClientFactory"/>
        /// </summary>
        /// <param name="tokenService">The token service to use for kafka</param>
        /// <param name="loggerFactory">The logger factory used to create necessary loggers</param>
        /// <param name="config">The configuration to use when creating clients</param>
        /// <param name="options">The options to use when creating clients</param>
        /// <param name="serializerProvider"></param>
        public KafkaClientFactory(
            ITokenService tokenService,
            ILoggerFactory loggerFactory,
            IOptions<KafkaConfig> config,
            ClientFactoryOptions options,
            ISerializerProvider serializerProvider)
        {
            _kafkaTokenService = tokenService;
            _loggerFactory = loggerFactory;
            _config = config.Value;
            _config.Validate();
            _options = options;
            _serializerProvider = serializerProvider;
            _logger = _loggerFactory.CreateLogger<KafkaClientFactory>();
        }

        /// <summary>
        /// Creates a pre-configured <see cref="IProducer{TKey,TValue}"/>/>
        /// </summary>
        /// <param name="producerOptions">Optional producer options used to modify the configuration</param>
        /// <typeparam name="TKey">The type of message key that the resulting producer will produce</typeparam>
        /// <typeparam name="TValue">The type of message value that the resulting producer will produce</typeparam>
        /// <returns>A pre-configured <see cref="IProducer{TKey,TValue}"/></returns>
        public IProducer<TKey, TValue> CreateProducer<TKey, TValue>(ProducerOptions<TKey, TValue>? producerOptions = null)
        {
            return CreateProducerBuilder<TKey, TValue>(producerOptions).Build();
        }

        /// <summary>
        /// Creates a pre-configured <see cref="ProducerBuilder{TKey,TValue}"/>/>
        /// </summary>
        /// <inheritdoc cref="CreateConsumer{TKey, TValue}"/>
        /// <returns>A pre-configured <see cref="ProducerBuilder{TKey, TValue}"/></returns>
        public ProducerBuilder<TKey, TValue> CreateProducerBuilder<TKey, TValue>(ProducerOptions<TKey, TValue>? producerOptions = null)
        {
            
            var configInstance = new ProducerConfig(GetDefaultConfig());
            _options.ProducerConfigure(configInstance);
            
            var builder = new ProducerBuilder<TKey, TValue>(configInstance);

            producerOptions ??= new ProducerOptions<TKey, TValue>();
            producerOptions.ConfigureAction?.Invoke(configInstance);
            producerOptions.BuilderAction?.Invoke(builder);
            var serializer = producerOptions.Serializer ?? _serializerProvider.GetSerializer<TValue>();
            
            return builder
                .AddCheetahOAuthentication(GetTokenRetrievalFunction(), _loggerFactory.CreateLogger<IProducer<TKey, TValue>>())
                .SetValueSerializer(serializer);
        }
        
        /// <summary>
        /// Creates a pre-configured <see cref="IProducer{TKey,TValue}"/> using a AvroSerializer as default/>
        /// </summary>
        /// <param name="producerOptions">Optional producer options used to modify the configuration</param>
        /// <typeparam name="TKey">The type of message key that the resulting producer will produce</typeparam>
        /// <typeparam name="TValue">The type of message value that the resulting producer will produce</typeparam>
        /// <returns>A pre-configured <see cref="IProducer{TKey,TValue}"/></returns>
        public IProducer<TKey, TValue> CreateAvroProducer<TKey, TValue>(ProducerOptions<TKey, TValue>? producerOptions = null)
        {
            return CreateAvroProducerBuilder<TKey, TValue>(producerOptions).Build();
        }

        /// <summary>
        /// Creates a pre-configured <see cref="ProducerBuilder{TKey,TValue}"/> Using AvroSerializer as default/>;
        /// </summary>
        /// <param name="producerOptions">Optional producer options used to modify the configuration</param>
        /// <typeparam name="TKey">The type of message key that the resulting producer will produce</typeparam>
        /// <typeparam name="TValue">The type of message value that the resulting producer will produce</typeparam>
        /// <returns>A pre-configured <see cref="ProducerBuilder{TKey, TValue}"/></returns>
        public ProducerBuilder<TKey, TValue> CreateAvroProducerBuilder<TKey, TValue>(
            ProducerOptions<TKey, TValue>? producerOptions = null)
        {
            producerOptions ??= new ProducerOptions<TKey, TValue>();
            if (producerOptions.Serializer != null)
            {
                return CreateProducerBuilder(producerOptions);
            }
            
            var authHeaderValueProvider = new OAuthHeaderValueProvider(_kafkaTokenService);
            producerOptions.SetSerializer(
                new AvroSerializer<TValue>(new CachedSchemaRegistryClient(_config.GetSchemaRegistryConfig(), authHeaderValueProvider)).AsSyncOverAsync());

            return CreateProducerBuilder(producerOptions);
        }
        
        /// <summary>
        /// Creates a pre-configured <see cref="IConsumer{TKey,TValue}"/>/>
        /// </summary>
        /// <param name="consumerOptions">Optional consumer option used to modify the configuration</param>
        /// <typeparam name="TKey">The type of message key that the resulting consumer will consume</typeparam>
        /// <typeparam name="TValue">The type of message value that the resulting consumer will consume</typeparam>
        /// <returns>A pre-configured <see cref="IConsumer{TKey,TValue}"/></returns>
        public IConsumer<TKey, TValue> CreateConsumer<TKey, TValue>(ConsumerOptions<TKey, TValue>? consumerOptions = null)
        {
            return CreateConsumerBuilder(consumerOptions).Build();
        }

        /// <summary>
        /// Creates a pre-configured <see cref="ConsumerBuilder{TKey,TValue}"/>/>
        /// </summary>
        /// <inheritdoc cref="CreateConsumer{TKey, TValue}"/>
        /// <returns>A pre-configured <see cref="ConsumerBuilder{TKey,TValue}"/></returns>
        public ConsumerBuilder<TKey, TValue> CreateConsumerBuilder<TKey, TValue>(ConsumerOptions<TKey, TValue>? consumerOptions = null)
        {
            var config = new ConsumerConfig(GetDefaultConfig());
            _options.ConsumerConfigure(config);
            
            var builder = new ConsumerBuilder<TKey, TValue>(config);
            consumerOptions ??= new ConsumerOptions<TKey, TValue>();
            consumerOptions.ConfigureAction?.Invoke(config);
            consumerOptions.BuilderAction?.Invoke(builder);
            var deserializer = consumerOptions.Deserializer ?? _serializerProvider.GetDeserializer<TValue>();
            
            return builder
                .AddCheetahOAuthentication(GetTokenRetrievalFunction(), _loggerFactory.CreateLogger<IConsumer<TKey, TValue>>())
                .SetValueDeserializer(deserializer);
        }
        
        /// <summary>
        /// Creates a pre-configured <see cref="ConsumerBuilder{TKey,TValue}"/> using a AvroDeserializer as default/>
        /// </summary>
        /// <param name="consumerOptions">Optional consumer option used to modify the configuration</param>
        /// <typeparam name="TKey">The type of message key that the resulting consumer will consume</typeparam>
        /// <typeparam name="TValue">The type of message value that the resulting consumer will consume</typeparam>
        /// <returns>A pre-configured <see cref="IConsumer{TKey,TValue}"/></returns>
        public IConsumer<TKey, TValue> CreateAvroConsumer<TKey, TValue>(ConsumerOptions<TKey, TValue>? consumerOptions = null)
        {
            return CreateAvroConsumerBuilder(consumerOptions).Build();
        }
        
        /// <summary>
        /// Creates a pre-configured <see cref="ConsumerBuilder{TKey,TValue}"/> using a AvroDeserializer as default/>
        /// </summary>
        /// <inheritdoc cref="CreateConsumer{TKey, TValue}"/>
        /// <returns>A pre-configured <see cref="ConsumerBuilder{TKey,TValue}"/></returns>
        public ConsumerBuilder<TKey, TValue> CreateAvroConsumerBuilder<TKey, TValue>(ConsumerOptions<TKey, TValue>? consumerOptions = null)
        {
            consumerOptions ??= new ConsumerOptions<TKey, TValue>();
            
            var authHeaderValueProvider = new OAuthHeaderValueProvider(_kafkaTokenService);
            var schemaRegistryClient = new CachedSchemaRegistryClient(
                _config.GetSchemaRegistryConfig(),
                authHeaderValueProvider
            );
            consumerOptions.SetDeserializer(
                new AvroDeserializer<TValue>(schemaRegistryClient).AsSyncOverAsync()
            );
            
            return CreateConsumerBuilder(consumerOptions);
        }

        /// <summary>
        /// Creates a pre-configured <see cref="IAdminClient"/>/>
        /// </summary>
        /// <param name="adminOptions">Optional admin option to modify the used <see cref="AdminClientConfig"/></param>
        /// <returns>A pre-configured <see cref="IAdminClient"/></returns>
        public IAdminClient CreateAdminClient(AdminClientOptions? adminOptions = null)
        {
            return CreateAdminClientBuilder(adminOptions).Build();
        }

        /// <summary>
        /// Creates a pre-configured <see cref="AdminClientBuilder"/>/>
        /// </summary>
        /// <param name="adminOptions">Optional admin option to modify the used <see cref="AdminClientConfig"/></param>
        /// <returns>A pre-configured <see cref="AdminClientBuilder"/></returns>
        public AdminClientBuilder CreateAdminClientBuilder(AdminClientOptions? adminOptions = null)
        {
            var config = new AdminClientConfig(GetDefaultConfig());
            _options.AdminClientConfigure(config);
            
            var builder = new AdminClientBuilder(config);
            adminOptions ??= new AdminClientOptions();
            adminOptions.ConfigureAction?.Invoke(config);
            adminOptions.BuilderAction?.Invoke(builder);
            
            return new AdminClientBuilder(config)
                .AddCheetahOAuthentication(GetTokenRetrievalFunction(), _loggerFactory.CreateLogger<IAdminClient>());
        }

        // Developer note:
        // The configuration structure used here ensures that a new instance of the configuration is created for each client.
        // This is important because using the copy-constructors provided by Confluent.Kafka will modify the underlying dictionary in-place.
        // This means that if we were to use the same configuration instance for each client, we would end up with a situation where
        // the configuration for the first client would be modified by the second client, and so on.
        private ClientConfig GetDefaultConfig() => _config.GetClientConfig();

        private Func<Task<(string AccessToken, long Expiration, string Principal)>> GetTokenRetrievalFunction()
        {
            return async () =>
            {
                var response = await _kafkaTokenService.RequestAccessTokenAsync(CancellationToken.None);
                return (response.AccessToken, response.Expiration, _config.Principal);
            };
        }
    }
}
